﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Coolite.Ext.Web;
using FireWorkflow.Net.Engine;
using FireWorkflow.Net.Kernel;
using WebDemo.Components;
using WebDemo.Example.GoodsDeliverProcess.Persistence;

namespace WebDemo.Example.GoodsDeliverProcess
{
    public partial class Payment : System.Web.UI.Page
    {
        TradeInfo paymentInfo = null;
        TradeInfoDAO tradeInfoDao = new TradeInfoDAO();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                paymentInfo = new TradeInfo();
                Sn.Text = paymentInfo.Sn;
            }
        }

        public void Save_Click(object sender, AjaxEventArgs e)
        {
            // 一、执行业务业务操作，保存业务数据
            paymentInfo = new TradeInfo()
            {
                Sn = Sn.Text,
                GoodsName = GoodsName.SelectedItem.Value,
                UnitPrice = UnitPrice.Number,
                Quantity = (long)Quantity.Number,
                Amount = Amount.Number,
                CustomerName = CustomerName.Text,
                CustomerMobile = CustomerMobile.Text,
                CustomerPhoneFax = CustomerPhoneFax.Text
            };
            tradeInfoDao.save(paymentInfo);

            // 二、开始执行流程操作
            IWorkflowSession workflowSession = RuntimeContextExamples.GetRuntimeContext().getWorkflowSession();//.workflowRuntimeContext.getWorkflowSession();
            try
            {
                // 1、创建流程实例
                IProcessInstance procInst = workflowSession.createProcessInstance("Goods_Deliver_Process", this.User.Identity.Name);
                // 2、设置流程变量/业务属性字段
                procInst.setProcessInstanceVariable("sn", paymentInfo.Sn);// 设置交易顺序号
                procInst.setProcessInstanceVariable("goodsName", paymentInfo.GoodsName);// 货物名称
                procInst.setProcessInstanceVariable("quantity", paymentInfo.Quantity);// 数量
                procInst.setProcessInstanceVariable("mobile", paymentInfo.CustomerMobile);// 客户电话
                procInst.setProcessInstanceVariable("customerName", paymentInfo.CustomerName);

                // 3、启动流程实例,run()方法启动实例并创建第一个环节实例、分派任务
                procInst.run();
            }
            catch
            {
                throw;
            }

            paymentInfo = new TradeInfo();
        }
    }
}
