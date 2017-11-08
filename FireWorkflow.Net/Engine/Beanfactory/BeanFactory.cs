using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Engine.Beanfactory
{
    public class BeanFactory : IBeanFactory
    {
        #region IBeanFactory 成员

        public object GetBean(string beanName)
        {
            Type type = Type.GetType(beanName);
            if (type != null) return Activator.CreateInstance(type, null);
            return null;
        }

        #endregion
    }
}
