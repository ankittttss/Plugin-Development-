using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace MyCustomWorkflow
{

    /// <summary>
    /// CodeActivity -: A base class for defining custom workflow activities.
    /// </summary>
    public class GetTaxWorkflow : CodeActivity
    {
        [Input("Key")]
        public InArgument<string> Key { get; set; }

        [Output("Tax")]
        public OutArgument<string> Tax { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                // Retrieve input argument value
                string key = Key.Get(executionContext);
                tracingService.Trace("Key retrieved: {0}", key);

                // Build the query
                QueryByAttribute queryByAttribute = new QueryByAttribute("nw_configuration")
                {
                    ColumnSet = new ColumnSet("nw_value")
                };
                queryByAttribute.AddAttributeValue("nw_name", key);

                // Retrieve the data
                EntityCollection entityCollection = service.RetrieveMultiple(queryByAttribute);

                // Ensure exactly one result
                if (entityCollection.Entities.Count != 1)
                {
                    tracingService.Trace("Expected exactly one configuration record, but found {0}.", entityCollection.Entities.Count);
                    throw new InvalidPluginExecutionException("Configuration error: Unable to retrieve the correct configuration value.");
                }

                // Retrieve and set the tax value
                Entity config = entityCollection.Entities[0];
                if (config.Attributes.Contains("nw_value"))
                {
                    string taxValue = config.Attributes["nw_value"].ToString();
                    tracingService.Trace("Retrieved Tax value: {0}", taxValue);
                    Tax.Set(executionContext, taxValue);
                }
                else
                {
                    tracingService.Trace("The 'nw_value' attribute is missing.");
                    throw new InvalidPluginExecutionException("Configuration error: 'nw_value' attribute is missing.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Exception: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in the custom workflow activity.", ex);
            }
        }
    }
}
