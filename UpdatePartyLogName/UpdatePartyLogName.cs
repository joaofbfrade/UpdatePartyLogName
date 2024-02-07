using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace UpdatePartyLogName
{
    public class UpdatePartyLogName : IPlugin
    {
        public IPluginExecutionContext _context = null;
        public IOrganizationServiceFactory _serviceFactory = null;
        public IOrganizationService service = null;
        public ITracingService trace = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = _serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Entered the plugin.");

            // ... switch on the MessageName
            //     and call actions to handle the supported messages
            switch (_context.MessageName.ToUpperInvariant())
            {
                
                //case "CREATE":
                //  OnCreate();
                // break;
                case "UPDATE":
                    trace.Trace("update");
                    OnUpdate();
                    break;
                //case "DELETE":
                //    OnDelete();
                //    break;
                default:
                    trace.Trace(_context.MessageName);
                    break;
            }
        }

        private void OnUpdate()
        {
            trace.Trace("update");


            if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
            {

                trace.Trace("if");

                // Get Record

                Entity updatedDocumentPartyLog = (Entity)_context.InputParameters["Target"];


                if (updatedDocumentPartyLog.LogicalName == "arq_documentpartylog")
                {
                    //New Record
                    trace.Trace("1");


                    Guid userid;
                    String name = "";

                    EntityReference relatedEntityUser = updatedDocumentPartyLog.GetAttributeValue<EntityReference>("arq_relateduser");
                    EntityReference relatedEntityEntity = updatedDocumentPartyLog.GetAttributeValue<EntityReference>("arq_relatedentity");
                    //EntityReference relatedEntityDoc= updatedDocumentPartyLog.GetAttributeValue<EntityReference>("arq_document");

                    trace.Trace("2");
                    trace.Trace(updatedDocumentPartyLog.Id.ToString());
                    //trace.Trace(relatedEntityDoc.Id.ToString());  

                    
                    




                    if (relatedEntityUser == null && relatedEntityEntity==null )
                    {
                        Entity myentity2 = service.Retrieve(updatedDocumentPartyLog.LogicalName, updatedDocumentPartyLog.Id, new ColumnSet("arq_document"));
                        var iddoclog = myentity2.GetAttributeValue<EntityReference>("arq_document").Id;


                        Entity myentity3 = service.Retrieve("arq_documentlog", iddoclog, new ColumnSet("arq_docfromparty"));
                        var iddocfrom = myentity3.GetAttributeValue<EntityReference>("arq_docfromparty").Id;

                        if (iddocfrom == updatedDocumentPartyLog.Id)
                        {
                            trace.Trace("from");

                            name = "-From";
                        }
                        else
                        {
                            trace.Trace("to");

                            name = "-To";
                        }



                        //doclogid = documentlog.GetAttributeValue<String>("firstname")


                    }











                    if (relatedEntityUser != null)
                    {
                        trace.Trace("user");
                        userid = relatedEntityUser.Id;
                        ColumnSet columns = new ColumnSet("firstname", "lastname");
                        Entity docRecord = service.Retrieve("systemuser", userid, columns);
                        name = "-" + docRecord.GetAttributeValue<String>("firstname") + " " + docRecord.GetAttributeValue<String>("lastname");
                       


                    }
                    else if (relatedEntityEntity != null)
                    {

                        userid = relatedEntityEntity.Id;
                        ColumnSet columns = new ColumnSet("name");
                        Entity docRecord = service.Retrieve("account", userid, columns);
                        name = "-" + docRecord.GetAttributeValue<String>("name");

                    }
                    else
                    {

                    }

                    Entity myentity = service.Retrieve(updatedDocumentPartyLog.LogicalName, updatedDocumentPartyLog.Id, new ColumnSet("arq_name"));
                    var prename = myentity.GetAttributeValue<string>("arq_name").ToString();

                    if (char.IsDigit(prename.Last()))
                    {
                        myentity["arq_name"] = prename + name;
                    }

                    else if (char.IsLetter(prename.Last()))
                    {

                        String[] vector = prename.Split('-');
                        Array.Resize(ref vector, vector.Length - 1);
                        string result = string.Join("-", vector);
                        myentity["arq_name"] = result + name;

                    }
                    else
                    {
                        String[] vector = prename.Split('-');
                        Array.Resize(ref vector, vector.Length - 1);                        
                        string result = string.Join("-", vector);
                        myentity["arq_name"] = result;

                    }


                    

                    service.Update(myentity);
                   


                }
            }

            trace.Trace("else");
        }
    }
}
        
