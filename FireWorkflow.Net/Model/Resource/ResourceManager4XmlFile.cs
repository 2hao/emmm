using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Model.Resource
{
    public class ResourceManager4XmlFile : IResourceManager 
    {
        List<Form> forms = null;
        List<Participant> participants = null;
        List<Application> applications = null;  

        #region IResourceManager Members

        public List<Application> getApplications()
        {
            return this.applications; 
        }

        public List<Participant> getParticipants()
        {
            return this.participants;
        }

        public List<Form> getForms()
        {
            return this.forms; 
        }

        #endregion
    }
}
