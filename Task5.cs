using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace My_Plugin
{
    public class Task5 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity account = (Entity)context.InputParameters["Target"];

                try
                {
                    // Plug-in business logic goes here. 

                    // Retrieve custom fields (ensure these fields exist on the entity).
                    string firstname = account.Contains("nw_fullname") ? account["nw_fullname"].ToString() : string.Empty;
                    string lastname = account.Contains("nw_lastname") ? account["nw_lastname"].ToString() : string.Empty;

                    // Create a new contact entity.
                    Entity contact = new Entity("contact");
                    contact.Attributes.Add("firstname", firstname);
                    contact.Attributes.Add("lastname", lastname);
                    contact.Attributes.Add("mobilephone", "87654322");
                    contact.Attributes.Add("emailaddress1", "ankitsaini9678@gmail.com");

                    // Set the parent customer ID to the created account.
                    contact.Attributes.Add("parentcustomerid", new EntityReference("account", account.Id));

                    // Create the contact record in the system.
                    Guid contactId = service.Create(contact);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("MyPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
