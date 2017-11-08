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
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Engine.Impl
{
    public class ProcessInstanceVar
    {
        public ProcessInstanceVarPk VarPrimaryKey { get; set; }
        public String ValueType { get; set; }
        public String StringValue { get; set; }
        public object Value
        {
            get
            {
                switch (ValueType)
                {
                    case "Int32": return Int32.Parse(this.StringValue);
                    case "Int64": return Int64.Parse(this.StringValue);
                    case "String": return this.StringValue;
                    case "Single": return Single.Parse(this.StringValue);
                    case "Double": return Double.Parse(this.StringValue);
                    case "Boolean": return Boolean.Parse(this.StringValue);
                    case "DateTime":
                        try { return DateTime.Parse(this.StringValue); }
                        catch { return null; }
                    default: throw new Exception("Fireflow不支持数据类型" + ValueType); ;
                }
            }
            set
            {
                if (String.IsNullOrEmpty(ValueType)) this.ValueType = value.GetType().Name;
                this.StringValue = value.ToString();
            }
        }

        public String Name { get { return VarPrimaryKey == null ? null : VarPrimaryKey.Name; } }

        public String ProcessInstanceId { get { return VarPrimaryKey == null ? null : VarPrimaryKey.ProcessInstanceId; } }


        public override Boolean Equals(Object obj)
        {
            if (this == obj) return true;
            if (!(obj is ProcessInstanceVar)) return false;
            ProcessInstanceVar var = (ProcessInstanceVar)obj;
            if (var.VarPrimaryKey.Equals(this.VarPrimaryKey))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (this.VarPrimaryKey == null)
            {
                return base.GetHashCode();
            }
            else
            {
                return this.VarPrimaryKey.GetHashCode();
            }
        }
    }
}
