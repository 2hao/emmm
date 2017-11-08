﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Coolite.Ext.Web;
using FireWorkflow.Net.Engine;
using FireWorkflow.Net.Engine.Persistence;
using FireWorkflow.Net.Kernel;
using WebDemo.Components;
using WebDemo.Example.LoanProcess.Persistence;

namespace WebDemo.Example.LoanProcess
{
    public partial class RiskEvaluateInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (this.Request.QueryString["WorkItemId"] != null)
                {
                    string workItemId = this.Request.QueryString["WorkItemId"];
                    IWorkflowSession wflsession = RuntimeContextExamples.GetRuntimeContext().getWorkflowSession();
                    IWorkItem wi = wflsession.findWorkItemById(workItemId);
                    String sn = (String)wi.TaskInstance.AliveProcessInstance.getProcessInstanceVariable("sn");
                    LoanInfoDAO lid = new LoanInfoDAO();
                    LoanInfo ti = lid.findBySn(sn);
                    if (ti != null)
                    {
                        HWorkItemId.Value = workItemId;
                        applicantName.Text = ti.ApplicantName;
                        applicantId.Text = ti.ApplicantId;
                        address.Text = ti.Address;
                        salary.Text = ti.Salary.ToString();
                        loanValue.Text = ti.LoanValue.ToString();
                        returnDate.Text = ti.ReturnDate;
                        loanteller.Text = ti.Loanteller;
                        appInfoInputDate.Text = ti.AppInfoInputDate.ToString("yyyy-MM-dd");

                        riskEvaluator.Text = this.User.Identity.Name;
                        riskInfoInputDate.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    }
                }
            }
        }

        public void Save_Click(object sender, AjaxEventArgs e)
        {
            string workItemId = HWorkItemId.Value.ToString();
            IWorkflowSession wflsession = RuntimeContextExamples.GetRuntimeContext().getWorkflowSession();
            IWorkItem wi = wflsession.findWorkItemById(workItemId);
            String sn = (String)wi.TaskInstance.AliveProcessInstance.getProcessInstanceVariable("sn");
            LoanInfoDAO lid = new LoanInfoDAO();
            LoanInfo loanInfo = lid.findBySn(sn);
            loanInfo.SalaryIsReal = Boolean.Parse(salaryIsReal.SelectedItem.Value);
            loanInfo.CreditStatus = Boolean.Parse(creditStatus.SelectedItem.Value);
            loanInfo.RiskEvaluator = riskEvaluator.Text;
            loanInfo.RiskInfoInputDate = DateTime.Parse(riskInfoInputDate.Text);


            //1、首先通过"收入状况是否属实"和"信用状况是否合格"这两项指标来设置RiskFlag流程变量
            if (loanInfo.SalaryIsReal && loanInfo.CreditStatus)
            {
                loanInfo.RiskFlag = false;//false表示风险评估通过
            }
            else
            {
                loanInfo.RiskFlag = true;//表示有风险
            }
            //2、将RiskFlag设置到流程变量
            try
            {
                wi.TaskInstance.AliveProcessInstance.setProcessInstanceVariable("RiskFlag", loanInfo.RiskFlag);
            }
            catch
            {
                throw;
            }

            //3、保存业务数据
            lid.attachDirty(loanInfo);
            try
            {
                if (wi != null)
                {
                    if (wi.ActorId == this.User.Identity.Name)
                    {
                        wi.complete(comments.Text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }

}
