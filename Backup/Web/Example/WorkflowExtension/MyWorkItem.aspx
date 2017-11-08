﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyWorkItem.aspx.cs" Inherits="WebDemo.Example.WorkflowExtension.MyWorkItem" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <ext:ScriptManager ID="ScriptManager1" runat="server" />
    
    <script type="text/javascript">
        Ext.onReady(function() {
        //top.Ext.WindowMgr.register(WindowView);
        });
        var prepare = function(grid, toolbar, rowIndex, record) {
        var claim = toolbar.items.items[0];
        var complete = toolbar.items.items[1];

            if (record.data.State == "INITIALIZED") {
                claim.show();
                complete.hide();
            }
            else {
                claim.hide();
                complete.show();
            }
        }
        var click = function(command, record, rowIndex, colIndex) {
            if (command == "claim") {
                Coolite.AjaxMethods.claim(record.data.Id, {
                    eventMask: { showMask: true, msg: '签收中...' },
                    success: function(result) {
                        if (!result) { Ext.Msg.alert('错误', '签收失败'); }
                    },
                    failure: function(errorMsg) {
                        Ext.Msg.alert('错误', errorMsg);
                    }
                });
            } else if (command == "complete") {
                Coolite.AjaxMethods.complete(record.data.Id, {
                    eventMask: { showMask: true, msg: '加载中...' },
                    failure: function(errorMsg) { Ext.Msg.alert('错误', errorMsg); }
                });
            }
        }
    </script>
    
    <ext:Store ID="Sdate" runat="server" OnRefreshData="Sdate_Refresh">
        <Reader>
            <ext:JsonReader ReaderID="Id">
                <Fields>
                    <ext:RecordField Name="Id" />
                    <ext:RecordField Name="Name" />
                    <ext:RecordField Name="DisplayName" />
                    <ext:RecordField Name="BizInfo" />
                    <ext:RecordField Name="State" />
                    <ext:RecordField Name="CreatedTime" Type="Date" />
                </Fields>
            </ext:JsonReader>
        </Reader>
    </ext:Store>


    <ext:Hidden ID="HProcessId" runat="server" />
    <ext:ViewPort ID="ViewPort1" runat="server">
        <Body>
            <ext:FitLayout ID="FitLayout1" runat="server">
                <ext:Panel ID="Panel1" runat="server" Height="300" Border="false" HideBorders="true" Title="我的待办工作">
                    <TopBar>
                        <ext:Toolbar ID="Toolbar1" runat="server">
                            <Items>
                                <ext:ToolbarButton ID="ToolbarButton3" runat="server" Text="刷新">
                                    <AjaxEvents>
                                        <Click OnEvent="query_Click">
                                            <EventMask ShowMask="true" Msg="正在查询数据请稍等..." MinDelay="100" />
                                        </Click>
                                    </AjaxEvents>
                                </ext:ToolbarButton>
                            </Items>
                        </ext:Toolbar>
                    </TopBar>
                    <Body>
                        <ext:FitLayout ID="CenterLayout2" runat="server">
                            <ext:GridPanel ID="mpgList" runat="server" StoreID="Sdate" ClicksToEdit="1" StripeRows="true" AutoExpandColumn="BizInfo">
                                <ColumnModel ID="ColumnModel1" runat="server">
                                    <Columns>
                                        <ext:CommandColumn Width="60" Header="任务操作">
                                            <Commands>
                                                <ext:GridCommand Icon="Note" CommandName="claim" Text="签收" />
                                                <ext:GridCommand Icon="NoteEdit" CommandName="complete" Text="完成" />
                                            </Commands>
                                            <PrepareToolbar Fn="prepare" />
                                        </ext:CommandColumn>
                                        <ext:Column Width="150px" Sortable="true" DataIndex="DisplayName" Header="环节说明">
                                        </ext:Column>
                                        <ext:Column Sortable="true" DataIndex="BizInfo" Header="业务信息">
                                        </ext:Column>
                                        <ext:Column Width="110px" Sortable="true" DataIndex="CreatedTime" Header="开始时间">
                                            <Renderer Fn="Ext.util.Format.dateRenderer('y-m-d h:i:s')" />
                                        </ext:Column>
                                    </Columns>
                                </ColumnModel>
                                <SelectionModel>
                                    <ext:RowSelectionModel ID="RowSelectionModel1" SingleSelect="true" runat="server">
                                    </ext:RowSelectionModel>
                                </SelectionModel>
                                <SaveMask ShowMask="true" />
                                <LoadMask ShowMask="true" />
                                <Listeners>
                                    <Command Fn="click" />
                                </Listeners>
                            </ext:GridPanel>
                        </ext:FitLayout>
                    </Body>
                </ext:Panel>
            </ext:FitLayout>
        </Body>
    </ext:ViewPort>
    <ext:Window ID="WindowView" runat="server" Width="650" Height="450" Collapsible="True" Maximizable="true" Constrain="True"
        ShowOnLoad="false" Icon="Help" Title="流程" BodyStyle="padding: 5px;"  Modal="True">
        <AutoLoad Url="" Mode="IFrame" MaskMsg="加载流程。。。" ShowMask="true" />
        <Listeners><Hide Handler="#{mpgList}.reload();" /></Listeners>
    </ext:Window>
    </form>
</body>
</html>
