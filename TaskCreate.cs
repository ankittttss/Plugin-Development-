using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace My_Plugin
{
    //Iplugin is an interface here//
    public class TaskCreate:IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
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
                Entity contact = (Entity)context.InputParameters["Target"];


                try
                {
                    // Plug-in business logic goes here. 

                    Entity taskRecord = new Entity("task");  //In-Memory Object//

                    taskRecord.Attributes.Add("subject","Crm Course");
                    taskRecord.Attributes.Add("statecode", "Low");
                    taskRecord.Attributes.Add("description", "This is a Task");
                    taskRecord.Attributes.Add("scheduledend", DateTime.Now.AddDays(2));

                    //How to set Option set Value//
                    taskRecord.Attributes.Add("prioritycode", new OptionSetValue(2));

                    //setting look-up Value

                    //taskRecord.Attributes.Add("regardingobjectid",new EntityReference("contact",contact.Id));

                    // We are passing GUID

                    taskRecord.Attributes.Add("regardingobjectid", contact.ToEntityReference());

                    Guid Taskguid =  service.Create(taskRecord);

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
