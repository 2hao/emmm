using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using FireWorkflow.Net.Model;
using FireWorkflow.Net.Model.Io;
using FireWorkflow.Net.Engine.Definition;
using Coolite.Ext.Web;
using WebDemo.Components;

namespace WebDemo
{
    public partial class AddWorkflowProcess : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                query_Click(null,null);
            }
        }

        public void query_Click(object sender, AjaxEventArgs e)
        {
            Sdate.DataSource = RuntimeContextExamples.GetRuntimeContext().DefinitionService.GetAllLatestVersionsOfWorkflowDefinition();
            Sdate.DataBind();
        }

        public void ok_Click(object sender, AjaxEventArgs e)
        {
            WorkflowDefinition wd = new WorkflowDefinition();
            //如是修改。
            string id = this.HProcessId.Value.ToString().Trim();
            if (!string.IsNullOrEmpty(id))
            {
                wd = RuntimeContextExamples.GetRuntimeContext().PersistenceService.FindWorkflowDefinitionById(id);
            }
            else
            {
                wd.PublishTime = DateTime.Now;
                wd.PublishUser = "admin";
            }
            wd.UploadTime = DateTime.Now;
            wd.UploadUser = "admin";
            wd.State = Boolean.Parse(this.state.SelectedItem.Value);

            if (this.BasicField.HasFile)
            {
                string filename = this.Server.MapPath("~/WorkFlowTemp/" + BasicField.PostedFile.FileName);
                BasicField.PostedFile.SaveAs(filename);

                WorkflowProcess workflowProcess;
                using (Stream inStream = new FileStream(filename, FileMode.Open))
                {
                    if (inStream == null)
                    {
                        throw new IOException("没有找到名称为" + filename + "的流程定义文件");
                    }
                    Dom4JFPDLParser parser = new Dom4JFPDLParser();
                    workflowProcess = parser.parse(inStream);
                }
                if (workflowProcess != null)
                {
                    if (string.IsNullOrEmpty(id)) wd.ProcessId = workflowProcess.Id;
                    wd.Name = workflowProcess.Name;
                    wd.DisplayName = workflowProcess.DisplayName;
                    wd.Description = workflowProcess.Description;
                    wd.ProcessContent = File.ReadAllText(filename);// twd.ProcessContent;
                }
                else
                {
                    Ext.Msg.Show(new MessageBox.Config
                    {
                        Buttons = MessageBox.Button.OK,
                        Icon = MessageBox.Icon.INFO,
                        Title = "Success",
                        Message = "错误的流程文件。"
                    });
                    return;
                }
            }

            if (RuntimeContextExamples.GetRuntimeContext().PersistenceService.SaveOrUpdateWorkflowDefinition(wd))
            {
                WorkflowEdit.Hide();

                Ext.Msg.Show(new MessageBox.Config
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.INFO,
                    Title = "Success",
                    Message = "保存成功。"
                });

                query_Click(null, null);
            }
            else
            {
                Ext.Msg.Show(new MessageBox.Config
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.INFO,
                    Title = "Success",
                    Message = "保存出错。"
                });
            }
        }


        protected void update_Click(object sender, AjaxEventArgs e)
        {
            RowSelectionModel sm = this.mpgList.SelectionModel.Primary as RowSelectionModel;
            if (sm != null && sm.SelectedRows.Count == 1)
            {
                WorkflowDefinition wd=RuntimeContextExamples.GetRuntimeContext().PersistenceService.FindWorkflowDefinitionById(sm.SelectedRows[0].RecordID);
                if (wd != null)
                {
                    this.state.SetValue(wd.State.ToString());
                    this.HProcessId.SetValue(wd.Id);
                    WorkflowEdit.SetTitle("修改流程=" + wd.ProcessId);
                    WorkflowEdit.Show();
                }
            }
            else
            {
                Ext.Msg.Show(new MessageBox.Config
                {
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.INFO,
                    Title = "Success",
                    Message = "请先选择修改的流程文件."
                });
            }
        }

        
    }
}
