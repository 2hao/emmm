drop table T_FF_DF_WORKFLOWDEF cascade constraints;
drop table T_FF_HIST_TRACE cascade constraints;
drop table T_FF_RT_PROCESSINSTANCE cascade constraints;
drop table T_FF_RT_PROCINST_VAR cascade constraints;
drop table T_FF_RT_TASKINSTANCE cascade constraints;
drop table T_FF_RT_TOKEN cascade constraints;
drop table T_FF_RT_WORKITEM cascade constraints;
create table T_FF_DF_WORKFLOWDEF (ID varchar2(50 char) not null, definition_type varchar2(50 char) not null, PROCESS_ID varchar2(100 char) not null, NAME varchar2(100 char) not null, DISPLAY_NAME varchar2(128 char), DESCRIPTION varchar2(1024 char), VERSION number(10,0) not null, STATE number(1,0) not null, UPLOAD_USER varchar2(50 char), UPLOAD_TIME timestamp, PUBLISH_USER varchar2(50 char), PUBLISH_TIME timestamp, PROCESS_CONTENT clob, primary key (ID));
create table T_FF_HIST_TRACE (ID varchar2(50 char) not null, PROCESSINSTANCE_ID varchar2(50 char) not null, STEP_NUMBER number(10,0) not null, MINOR_NUMBER number(10,0) not null, TYPE varchar2(15 char) not null, EDGE_ID varchar2(100 char), FROM_NODE_ID varchar2(100 char) not null, TO_NODE_ID varchar2(100 char) not null, primary key (ID));
create table T_FF_RT_PROCESSINSTANCE (ID varchar2(50 char) not null, PROCESS_ID varchar2(100 char) not null, VERSION number(10,0) not null, NAME varchar2(100 char), DISPLAY_NAME varchar2(128 char), STATE number(10,0) not null, SUSPENDED number(1,0) not null, CREATOR_ID varchar2(50 char), CREATED_TIME timestamp, STARTED_TIME timestamp, EXPIRED_TIME timestamp, END_TIME timestamp, PARENT_PROCESSINSTANCE_ID varchar2(50 char), PARENT_TASKINSTANCE_ID varchar2(50 char), primary key (ID));
create table T_FF_RT_PROCINST_VAR (PROCESSINSTANCE_ID varchar2(50 char) not null, VALUE varchar2(255 char), NAME varchar2(255 char) not null, primary key (PROCESSINSTANCE_ID, NAME));
create table T_FF_RT_TASKINSTANCE (ID varchar2(50 char) not null, BIZ_TYPE varchar2(250 char) not null, TASK_ID varchar2(300 char) not null, ACTIVITY_ID varchar2(200 char) not null, NAME varchar2(100 char) not null, DISPLAY_NAME varchar2(128 char), STATE number(10,0) not null, SUSPENDED number(1,0) not null, TASK_TYPE varchar2(10 char), CREATED_TIME timestamp not null, STARTED_TIME timestamp, EXPIRED_TIME timestamp, END_TIME timestamp, ASSIGNMENT_STRATEGY varchar2(10 char), PROCESSINSTANCE_ID varchar2(50 char) not null, PROCESS_ID varchar2(100 char) not null, VERSION number(10,0) not null, TARGET_ACTIVITY_ID varchar2(100 char), FROM_ACTIVITY_ID varchar2(600 char), STEP_NUMBER number(10,0) not null, CAN_BE_WITHDRAWN number(1,0) not null, primary key (ID));
create table T_FF_RT_TOKEN (ID varchar2(50 char) not null, ALIVE number(1,0) not null, VALUE number(10,0) not null, NODE_ID varchar2(200 char) not null, PROCESSINSTANCE_ID varchar2(50 char) not null, STEP_NUMBER number(10,0) not null, FROM_ACTIVITY_ID varchar2(100 char), primary key (ID));
create table T_FF_RT_WORKITEM (ID varchar2(50 char) not null, STATE number(10,0) not null, CREATED_TIME timestamp not null, CLAIMED_TIME timestamp, END_TIME timestamp, ACTOR_ID varchar2(50 char), COMMENTS varchar2(1024 char), TASKINSTANCE_ID varchar2(50 char) not null, primary key (ID));
comment on column T_FF_RT_WORKITEM.STATE is '状态,0=Initialized ,1=Running, 7=Completed,9=Canceled';
comment on column T_FF_RT_WORKITEM.CREATED_TIME is '创建时间';
comment on column T_FF_RT_WORKITEM.CLAIMED_TIME is '签收时间';
comment on column T_FF_RT_WORKITEM.END_TIME is '结束时间';
comment on column T_FF_RT_WORKITEM.ACTOR_ID is '操作者Id';
comment on column T_FF_RT_WORKITEM.COMMENTS is '说明';
create index IDX_TRACE_PROCINSTID on T_FF_HIST_TRACE (PROCESSINSTANCE_ID);
alter table T_FF_RT_PROCINST_VAR add constraint FKD79C420D7AF471D8 foreign key (PROCESSINSTANCE_ID) references T_FF_RT_PROCESSINSTANCE;
create index idx_taskInst_stepNb on T_FF_RT_TASKINSTANCE (STEP_NUMBER);
create index idx_wi_creatorid on T_FF_RT_WORKITEM (ACTOR_ID);
alter table T_FF_RT_WORKITEM add constraint FK4131554DE2527DDC foreign key (TASKINSTANCE_ID) references T_FF_RT_TASKINSTANCE;


drop table T_Biz_TradeInfo cascade constraints;
drop table T_BIZ_LOANINFO cascade constraints;
drop table T_BIZ_LOAN_APPROVEINFO cascade constraints;
create table T_Biz_TradeInfo (ID varchar2(50 char) not null, SN varchar2(50 char) unique, GOODS_NAME varchar2(100 char), GOODS_TYPE varchar2(50 char), QUANTITY number(19,0), UNIT_PRICE double precision, AMOUNT double precision, CUSTOMER_NAME varchar2(50 char), CUSTOMER_MOBILE varchar2(30 char), CUSTOMER_PHONE_FAX varchar2(30 char), CUSTOMER_ADDRESS varchar2(150 char), STATE varchar2(15 char), PAYED_TIME timestamp, DELIVERED_TIME timestamp, primary key (ID));
create table T_BIZ_LOANINFO (ID varchar2(50 char) not null, SN varchar2(50 char) not null unique, APPLICANT_NAME varchar2(50 char) not null, APPLICANT_ID varchar2(50 char) not null, ADDRESS varchar2(256 char) not null, SALARY number(10,0) not null, LOAN_VALUE number(10,0) not null, RETURN_DATE varchar2(10 char) not null, LOANTELLER varchar2(50 char) not null, APP_INFO_INPUT_DATE timestamp not null, SALARY_IS_REAL number(1,0), CREDIT_STATUS number(1,0), RISK_FLAG number(1,0), RISK_EVALUATOR varchar2(50 char), RISK_INFO_INPUT_DATE timestamp, DECISION number(1,0), examinerList varchar2(128 char), approverList varchar2(128 char), opponentList varchar2(128 char), LEND_MONEY_INFO varchar2(256 char), LEND_MONEY_OFFICER varchar2(50 char), LEND_MONEY_INFO_INPUT_TIME timestamp, REJECT_INFO varchar2(256 char), REJECT_INFO_INPUT_TIME timestamp, primary key (ID));
create table T_BIZ_LOAN_APPROVEINFO (ID varchar2(50 char) not null, SN varchar2(50 char) not null, approver varchar2(50 char) not null, decision number(1,0), detail varchar2(50 char), primary key (ID));
