﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FireWorkflow.Net.Engine;
using FireWorkflow.Net.Engine.Impl;
using FireWorkflow.Net.Engine.Definition;

namespace WebDemo
{
    // 注意: 如果更改此处的类名 "DesignerService"，也必须更新 Web.config 中对 "DesignerService" 的引用。
    public class DesignerService : IDesignerService
    {
        public String GetWorkflowProcessXml(String id)
        {
            WorkflowDefinition wd = RuntimeContextFactory.getRuntimeContext().PersistenceService.FindWorkflowDefinitionById(id);
            if (wd != null) return wd.ProcessContent;
            else return "";
        }

        public List<ProcessInstanceTrace> GetProcessInstanceTraceXml(String processInstanceId)
        {
            List<ProcessInstanceTrace> pit = RuntimeContextFactory.getRuntimeContext().PersistenceService.FindProcessInstanceTraces(processInstanceId);
            return pit;
        }
    }
}
