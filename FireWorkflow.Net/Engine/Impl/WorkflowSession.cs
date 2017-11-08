﻿/**
 * Copyright 2003-2008 非也
 * All rights reserved. 
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation。
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses. *
 * @author 非也,nychen2000@163.com
 * @Revision to .NET 无忧 lwz0721@gmail.com 2010-02
 */
using System;
using System.Collections.Generic;
using System.Text;
using FireWorkflow.Net.Model;
using FireWorkflow.Net.Engine;
using FireWorkflow.Net.Engine.Taskinstance;
using FireWorkflow.Net.Engine.Definition;
using FireWorkflow.Net.Engine.Persistence;
using FireWorkflow.Net.Kernel;

namespace FireWorkflow.Net.Engine.Impl
{
    public class WorkflowSession : IWorkflowSession, IRuntimeContextAware
    {
        public RuntimeContext RuntimeContext { get; set; }
        protected DynamicAssignmentHandler dynamicAssignmentHandler = null;
        protected Boolean inWithdrawOrRejectOperation = false;
        protected Dictionary<String, Object> attributes = new Dictionary<String, Object>();

        public WorkflowSession(RuntimeContext ctx)
        {
            this.RuntimeContext = ctx;
        }

        public void setCurrentDynamicAssignmentHandler(DynamicAssignmentHandler handler)
        {
            this.dynamicAssignmentHandler = handler;
        }

        public DynamicAssignmentHandler consumeCurrentDynamicAssignmentHandler()
        {
            DynamicAssignmentHandler handler = this.dynamicAssignmentHandler;
            this.dynamicAssignmentHandler = null;
            return handler;
        }

        /// <summary>
        /// 创建流程实例。如果流程定义了流程变量(DataField）,则自动把流程变量初始化成流程实例变量。
        /// </summary>
        /// <param name="workflowProcessName">流程的Name属性，在Fire workflow中，流程的Name和Id是相等的而且都是唯一的。</param>
        /// <param name="parentTaskInstance">父TaskInstance</param>
        /// <returns>新创建的流程实例</returns>
        public IProcessInstance createProcessInstance(String workflowProcessName, ITaskInstance parentTaskInstance)
        {
            return _createProcessInstance(workflowProcessName, parentTaskInstance.Id,
                parentTaskInstance.ProcessInstanceId, parentTaskInstance.Id);
        }

        /// <summary>创建一个新的流程实例 (create a new process instance )</summary>
        /// <param name="workflowProcessId">流程定义ID</param>
        /// <param name="creatorId">创建人ID</param>
        /// <param name="parentProcessInstanceId">父流程实例ID</param>
        /// <param name="parentTaskInstanceId">父任务实例ID</param>
        protected IProcessInstance _createProcessInstance(String workflowProcessId, String creatorId, String parentProcessInstanceId, String parentTaskInstanceId)
        {
            String wfprocessId = workflowProcessId;

            WorkflowDefinition workflowDef = RuntimeContext.DefinitionService.GetTheLatestVersionOfWorkflowDefinition(wfprocessId);
            WorkflowProcess wfProcess = workflowDef.getWorkflowProcess();

            if (wfProcess == null)
            {
                throw new Exception("Workflow process NOT found,id=[" + wfprocessId + "]");
            }
            IProcessInstance processInstance = (IProcessInstance)this.execute(
                new WorkflowSessionIProcessInstanceCreateProcessInstance(creatorId, wfProcess, workflowDef, parentProcessInstanceId, parentTaskInstanceId));

            // 初始化流程变量
            processInstance.ProcessInstanceVariables = new Dictionary<String, Object>();

            List<DataField> datafields = wfProcess.DataFields;
            for (int i = 0; datafields != null && i < datafields.Count; i++)
            {
                DataField df = datafields[i];
                if (df.DataType == DataTypeEnum.STRING)
                {
                    if (df.InitialValue != null)
                    {
                        processInstance.setProcessInstanceVariable(df.Name, df.InitialValue);
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, "");
                    }
                }
                else if (df.DataType == DataTypeEnum.INTEGER)
                {
                    if (df.InitialValue != null)
                    {
                        try
                        {
                            int intValue = int.Parse(df.InitialValue);
                            processInstance.setProcessInstanceVariable(df.Name, intValue);
                        }
                        catch { }
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, 0);
                    }
                }
                else if (df.DataType == DataTypeEnum.LONG)
                {
                    if (df.InitialValue != null)
                    {
                        try
                        {
                            long longValue = long.Parse(df.InitialValue);
                            processInstance.setProcessInstanceVariable(df.Name, longValue);
                        }
                        catch { }
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, (long)0);
                    }
                }
                else if (df.DataType == DataTypeEnum.FLOAT)
                {
                    if (df.InitialValue != null)
                    {
                        float floatValue = float.Parse(df.InitialValue);
                        processInstance.setProcessInstanceVariable(df.Name, floatValue);
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, (float)0);
                    }
                }
                else if (df.DataType == DataTypeEnum.DOUBLE)
                {
                    if (df.InitialValue != null)
                    {
                        Double doubleValue = Double.Parse(df.InitialValue);
                        processInstance.setProcessInstanceVariable(df.Name, doubleValue);
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, (Double)0);
                    }
                }
                else if (df.DataType == DataTypeEnum.BOOLEAN)
                {
                    if (df.InitialValue != null)
                    {
                        Boolean booleanValue = Boolean.Parse(df.InitialValue);
                        processInstance.setProcessInstanceVariable(df.Name, booleanValue);
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, false);
                    }
                }
                else if (df.DataType == DataTypeEnum.DATETIME)
                {
                    // TODO 需要完善一下
                    if (df.InitialValue != null && df.DataPattern != null)
                    {
                        try
                        {
                            DateTime dateTmp = DateTime.Parse(df.InitialValue);
                            processInstance.setProcessInstanceVariable(df.Name, dateTmp);
                        }
                        catch
                        {
                            processInstance.setProcessInstanceVariable(df.Name, null);
                        }
                    }
                    else
                    {
                        processInstance.setProcessInstanceVariable(df.Name, null);
                    }
                }
            }
            return processInstance;
        }

        /// <summary>
        /// <para>创建流程实例，并将creatorId填充到ProcessInstance的CreatorId字段。</para>
        /// 如果流程定义了流程变量(DataField）,则自动把流程变量初始化成流程实例变量。
        /// </summary>
        /// <param name="workflowProcessName">流程定义的名称</param>
        /// <param name="creatorId">创建者Id</param>
        /// <returns>新创建的流程实例</returns>
        public IProcessInstance createProcessInstance(String workflowProcessId, String creatorId)
        {
            return _createProcessInstance(workflowProcessId, creatorId, null, null);
        }

        /// <summary>通过workitem的id查找到该workitem</summary>
        /// <param name="id">workitem id</param>
        /// <returns>Workitem对象</returns>
        public IWorkItem findWorkItemById(String id)
        {
            String workItemId = id;
            try
            {
                return (IWorkItem)this.execute(new WorkflowSessionIWorkItem(id));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>模板方法</summary>
        /// <param name="callbak"></param>
        /// <returns>
        /// <para>返回的对象一般有三种情况：</para>
        /// <para>1、单个工作流引擎API对象（如IProcessInstance,ITaskInstance,IworkItem等）</para>
        /// <para>2、工作流引擎对象的列表、3、null</para>
        /// </returns>
        public Object execute(IWorkflowSessionCallback callback)
        {
            try
            {
                Object result = callback.doInWorkflowSession(RuntimeContext);
                if (result != null)
                {
                    if (result is IRuntimeContextAware)
                    {
                        ((IRuntimeContextAware)result).RuntimeContext = this.RuntimeContext;
                    }
                    if (result is IWorkflowSessionAware)
                    {
                        ((IWorkflowSessionAware)result).CurrentWorkflowSession = this;
                    }

                    if (result is List<Object>)
                    {
                        List<Object> l = (List<Object>)result;
                        for (int i = 0; i < l.Count; i++)
                        {
                            Object item = l[i];
                            if (item is IRuntimeContextAware)
                            {
                                ((IRuntimeContextAware)item).RuntimeContext = this.RuntimeContext;
                                if (item is IWorkflowSessionAware)
                                {
                                    ((IWorkflowSessionAware)item).CurrentWorkflowSession = this;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            finally
            {

            }
        }

        /// <summary>根据任务实例的Id查找任务实例。</summary>
        /// <param name="id">任务实例的Id</param>
        /// <returns></returns>
        public ITaskInstance findTaskInstanceById(String id)
        {
            String taskInstanceId = id;
            try
            {
                return (ITaskInstance)this.execute(new WorkflowSessionITaskInstance(taskInstanceId));
            }
            catch
            {
                return null;
            }
        }

        public List<IWorkItem> findMyTodoWorkItems(String actorId)
        {
            List<IWorkItem> result = null;
            try
            {
                result = (List<IWorkItem>)this.execute(new WorkflowSessionIWorkItems(actorId));
            }
            catch { }
            return result;
        }

        public List<IWorkItem> findMyTodoWorkItems(String actorId, String processInstanceId)
        {
            List<IWorkItem> result = null;
            try
            {
                result = (List<IWorkItem>)this.execute(new WorkflowSessionIWorkItems(actorId, processInstanceId));
            }
            catch { }
            return result;
        }

        public List<IWorkItem> findMyTodoWorkItems(String actorId, String processId, String taskId)
        {
            List<IWorkItem> result = null;
            try
            {
                result = (List<IWorkItem>)this.execute(new WorkflowSessionIWorkItems(actorId, processId, taskId));

            }
            catch { }
            return result;
        }

        /// <summary>设置当前WorkflowSession是在一个取回或者拒收的操作环境中。</summary>
        /// <param name="b">true表示是在一个取回或者拒收的操作环境中；false表示不是在取回或者拒收的操作环境中</param>
        public void setWithdrawOrRejectOperationFlag(Boolean b)
        {
            this.inWithdrawOrRejectOperation = b;
        }

        public Boolean isInWithdrawOrRejectOperation()
        {
            return this.inWithdrawOrRejectOperation;
        }

        public void setDynamicAssignmentHandler(DynamicAssignmentHandler dynamicAssignmentHandler)
        {
            this.dynamicAssignmentHandler = dynamicAssignmentHandler;
        }

        public IProcessInstance abortProcessInstance(String processInstanceId)
        {
            IProcessInstance processInstance = this.findProcessInstanceById(processInstanceId);
            processInstance.abort();
            return processInstance;
        }

        public IWorkItem claimWorkItem(String workItemId)
        {
            IWorkItem result = null;
            IWorkItem wi = this.findWorkItemById(workItemId);
            result = wi.claim();
            return result;
        }

        public void completeWorkItem(String workItemId)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.complete();

        }

        public void completeWorkItem(String workItemId, String comments)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.complete(comments);
        }

        public void completeWorkItem(String workItemId, DynamicAssignmentHandler dynamicAssignmentHandler, String comments)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.complete(dynamicAssignmentHandler, comments);

        }

        public void completeWorkItemAndJumpTo(String workItemId, String targetActivityId)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.jumpTo(targetActivityId);
        }

        public void completeWorkItemAndJumpTo(String workItemId, String targetActivityId, String comments)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.jumpTo(targetActivityId, comments);
        }

        public void completeWorkItemAndJumpTo(String workItemId, String targetActivityId,
                DynamicAssignmentHandler dynamicAssignmentHandler, String comments)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.jumpTo(targetActivityId, dynamicAssignmentHandler, comments);
        }

        public void completeWorkItemAndJumpToEx(String workItemId, String targetActivityId,
            DynamicAssignmentHandler dynamicAssignmentHandler, String comments)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            wi.jumpToEx(targetActivityId, dynamicAssignmentHandler, comments);
        }

        public IProcessInstance findProcessInstanceById(String id)
        {
            try
            {
                return (IProcessInstance)this.execute(new WorkflowSessionIProcessInstance1(id));
            }
            catch { return null; }
        }

        public List<IProcessInstance> findProcessInstancesByProcessId(String processId)
        {
            try
            {
                return (List<IProcessInstance>)this.execute(new WorkflowSessionIProcessInstances(processId));
            }
            catch { return null; }
        }

        public List<IProcessInstance> findProcessInstancesByProcessIdAndVersion(String processId, Int32 version)
        {
            try
            {
                return (List<IProcessInstance>)this.execute(new WorkflowSessionIProcessInstances(processId, version));

            }
            catch { return null; }
        }

        /// <summary>
        /// <para>查询流程实例的所有的TaskInstance,如果activityId不为空，则返回该流程实例下指定环节的TaskInstance</para>
        /// (Engine没有引用到该方法，提供给业务系统使用，20090303)
        /// </summary>
        /// <param name="processInstanceId">the id of the process instance</param>
        /// <param name="activityId">if the activityId is null, then return all the taskinstance of the processinstance;</param>
        /// <returns>符合条件的TaskInstance列表</returns>
        public List<ITaskInstance> findTaskInstancesForProcessInstance(String processInstanceId, String activityId)
        {
            try
            {
                return (List<ITaskInstance>)this.execute(new WorkflowSessionITaskInstances(processInstanceId, activityId));
            }
            catch { return null; }
        }

        /// <summary>将工作项委派给其他人，自己的工作项变成CANCELED状态</summary>    
        /// <param name="workItemId">工作项Id</param>
        /// <param name="actorId">接受任务的操作员Id</param>
        /// <returns>新创建的工作项</returns>
        public IWorkItem reasignWorkItemTo(String workItemId, String actorId)
        {
            IWorkItem workItem = this.findWorkItemById(workItemId);
            return workItem.reassignTo(actorId);
        }

        /// <summary>将工作项委派给其他人，自己的工作项变成CANCELED状态。返回新创建的工作项</summary>    
        /// <param name="workItemId">工作项Id</param>
        /// <param name="actorId">接受任务的操作员Id</param>
        /// <param name="comments">相关的备注信息</param>
        /// <returns>新创建的工作项</returns>
        public IWorkItem reasignWorkItemTo(String workItemId, String actorId, String comments)
        {
            IWorkItem workItem = this.findWorkItemById(workItemId);
            return workItem.reassignTo(actorId, comments);
        }

        /// <summary>
        /// <para>执行“拒收”操作，可以对已经签收的或者未签收的WorkItem拒收。</para>
        /// <para>该操作必须满足如下条件：</para>
        /// <para>1、前驱环节中没有没有Tool类型和Subflow类型的Task；</para>
        /// <para>2、没有合当前TaskInstance并行的其他TaskInstance；</para> 
        /// </summary>
        /// <param name="workItemId">工作项Id</param>
        public void rejectWorkItem(String workItemId)
        {
            IWorkItem workItem = this.findWorkItemById(workItemId);
            workItem.reject();
        }

        /// <summary>
        /// <para>执行“拒收”操作，可以对已经签收的或者未签收的WorkItem拒收。</para>
        /// <para>该操作必须满足如下条件：</para>
        /// <para>1、前驱环节中没有没有Tool类型和Subflow类型的Task；</para>
        /// <para>2、没有合当前TaskInstance并行的其他TaskInstance；</para>
        /// </summary>  
        /// <param name="workItemId">工作项Id</param>
        /// <param name="comments">备注信息</param>
        public void rejectWorkItem(String workItemId, String comments)
        {
            IWorkItem workItem = this.findWorkItemById(workItemId);
            workItem.reject(comments);
        }

        /// <summary>恢复被挂起的流程实例</summary>
        /// <param name="processInstanceId">流程实例Id</param>
        public IProcessInstance restoreProcessInstance(String processInstanceId)
        {
            IProcessInstance processInstance = this.findProcessInstanceById(processInstanceId);
            processInstance.restore();
            return processInstance;
        }

        /// <summary>恢复被挂起的TaskInstance</summary>
        /// <param name="taskInstanceId"></param>
        /// <returns></returns>
        public ITaskInstance restoreTaskInstance(String taskInstanceId)
        {
            ITaskInstance taskInst = this.findTaskInstanceById(taskInstanceId);
            taskInst.restore();
            return taskInst;
        }

        public IProcessInstance suspendProcessInstance(String processInstanceId)
        {
            IProcessInstance processInstance = this.findProcessInstanceById(processInstanceId);
            processInstance.suspend();
            return processInstance;
        }

        public ITaskInstance suspendTaskInstance(String taskInstanceId)
        {
            ITaskInstance taskInst = this.findTaskInstanceById(taskInstanceId);
            taskInst.suspend();
            return taskInst;
        }

        public IWorkItem withdrawWorkItem(String workItemId)
        {
            IWorkItem wi = this.findWorkItemById(workItemId);
            return wi.withdraw();
        }

        public void clearAttributes()
        {
            this.attributes.Clear();
        }

        public Object getAttribute(String name)
        {
            return this.attributes[name];
        }

        public void removeAttribute(String name)
        {
            this.attributes.Remove(name);
        }

        public void setAttribute(String name, Object attr)
        {
            this.attributes.Add(name, attr);
        }
    }
}
