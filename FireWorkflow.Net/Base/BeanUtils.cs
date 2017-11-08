using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FireWorkflow.Net.Base
{
    /// <summary>
    /// 复制对象
    /// </summary>
    public class BeanUtils
    {
        public static void CopyProperties(object pobSrc, object pobDest, OptionTyp penOpt)
        {
            SetProperties(GetProperties(pobSrc), pobDest, penOpt);
        }
        public static void CopyPropertiesWithMap(object pobSrc, object pobDest, Dictionary<String, string> pdiMap, OptionTyp penOpt)
        {
            List<String> strSrc = new List<String>();
            List<String> strDest = new List<String>();
            foreach (KeyValuePair<String, string> pair in pdiMap)
            {
                strSrc.Add(pair.Key);
                strDest.Add(pair.Value);
            }
            CopyPropertiesWithMap(pobSrc, pobDest, strSrc.ToArray(), strDest.ToArray(), penOpt);
        }
        public static void CopyPropertiesWithMap(object pobSrc, object pobDest, string[] pstSrcPropertyNames, string[] pstDestPropertyNames, OptionTyp penOpt)
        {
            if (null == pobSrc || null == pobDest)
            { throw new ArgumentNullException("one argument is null!"); }
            if (pstDestPropertyNames.Length != pstSrcPropertyNames.Length)
                throw new ArgumentException("pstDestPropertyNames & pstSrcPropertyNames must same array length");
            for (int i = 0; i < pstDestPropertyNames.Length; i++)
            {
                CopyProperty(pobSrc, pobDest, pstSrcPropertyNames[i], pstDestPropertyNames[i], penOpt);
            }
        }
        public static T GenernationObject<T>(object pobSrc, OptionTyp penOpt)
        {
            T lobDest = Activator.CreateInstance<T>();
            CopyProperties(pobSrc, lobDest, penOpt);
            return lobDest;
        }
        public static T GenernationObject<T>(Dictionary<String, object> pdiProperties, OptionTyp penOpt)
        {
            T lobDest = Activator.CreateInstance<T>();
            SetProperties(pdiProperties, lobDest, penOpt);
            return lobDest;
        }
        public static Dictionary<String, object> GetProperties(object pobObj)
        {
            Dictionary<String, object> list = new Dictionary<String, object>();
            string name;
            object val;
            if (null == pobObj) { throw new ArgumentNullException("pobObj can't be null"); }
            Type objType = pobObj.GetType();
            PropertyInfo[] objInfo = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objInfo.Length; i++)
            {
                name = objInfo[i].Name;
                val = objInfo[i].GetValue(pobObj, null);
                list.Add(name, val);
            }
            return list;
        }
        public static void SetProperties(Dictionary<String, object> pdiProperties, object pobObj, OptionTyp penOpt)
        {
            foreach (KeyValuePair<String, object> pair in pdiProperties)
            {
                try
                {
                    SetProperty(pobObj, pair.Key, pair.Value, penOpt);
                }
                catch (MapPropertyException) { }
            }
        }
        public static void CopyProperty(object pobSrc, object pobDest, string pstPropertyName, OptionTyp penOpt)
        {
            CopyProperty(pobSrc, pobDest, pstPropertyName, pstPropertyName, penOpt);
        }
        public static void CopyProperty(object pobSrc, object pobDest, string pstSrcPropertyName, string pstDestPropertyName, OptionTyp penOpt)
        {
            SetProperty(pobDest, pstDestPropertyName, GetProperty(pobSrc, pstSrcPropertyName, penOpt), penOpt);
        }
        public static void SetProperty(object pobObj, string pstPropertyName, object pobValue, OptionTyp penOpt)
        {
            if (null == pobObj || string.IsNullOrEmpty(pstPropertyName))
            {
                throw new ArgumentNullException("one argument is null!");
            }
            bool isIgnoreCase = ((penOpt & OptionTyp.IsIgnoreCase) == OptionTyp.IsIgnoreCase);
            bool isConvert = ((penOpt & OptionTyp.IsConvert) == OptionTyp.IsConvert);
            bool isThrowConvertException = ((penOpt & OptionTyp.IsThrowConvertException) == OptionTyp.IsThrowConvertException);
            Type t = pobObj.GetType();
            PropertyInfo objInfo = null;
            if (isIgnoreCase)
            {
                PropertyInfo[] objInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo p in objInfos)
                {
                    if (p.Name.ToUpperInvariant().Equals(pstPropertyName.ToUpperInvariant()))
                    {
                        objInfo = p;
                        break;
                    }
                }
            }
            else
            {
                objInfo = t.GetProperty(pstPropertyName, BindingFlags.Instance | BindingFlags.Public);
            }
            if (null == objInfo)
                throw new MapPropertyException("no mapping property");
            object descVal = null;
            if (null == pobValue || !isConvert)
            {
                descVal = pobValue;
            }
            else
            {
                Type srcPropertyType = pobValue.GetType();
                Type destPropertyType = objInfo.PropertyType;
                if (srcPropertyType.Equals(destPropertyType))
                {
                    descVal = pobValue;
                }
                else
                {
                    MethodInfo methodinfo = typeof(Convert).GetMethod("To" + destPropertyType.Name, new Type[] { srcPropertyType });
                    try
                    {
                        descVal = methodinfo.Invoke(null, new object[] { pobValue });
                    }
                    catch
                    {
                        if (isThrowConvertException)
                        {
                            throw new ConvertException("can't convert");
                        }
                        descVal = null;
                    }
                }
            }
            objInfo.SetValue(pobObj, descVal, null);
        }
        public static object GetProperty(object pobObj, string pstPropertyName, OptionTyp penOpt)
        {
            if (null == pobObj || string.IsNullOrEmpty(pstPropertyName))
            {
                throw new ArgumentNullException("one argument is null!");
            }
            bool isIgnoreCase = ((penOpt & OptionTyp.IsIgnoreCase) == OptionTyp.IsIgnoreCase);
            Type t = pobObj.GetType();
            PropertyInfo objInfo = null;
            if (isIgnoreCase)
            {
                PropertyInfo[] objInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo p in objInfos)
                {
                    if (p.Name.ToUpperInvariant().Equals(pstPropertyName.ToUpperInvariant()))
                    {
                        objInfo = p;
                        break;
                    }
                }
            }
            else
            {
                objInfo = t.GetProperty(pstPropertyName, BindingFlags.Instance | BindingFlags.Public);
            }
            if (null == objInfo)
                throw new MapPropertyException("no mapping property");
            object val = objInfo.GetValue(pobObj, null);
            return val;
        }
    }
    [Flags]
    public enum OptionTyp
    {
        None = 0,
        IsIgnoreCase = 0x0001,

        IsConvert = 0x0002,

        IsThrowConvertException = 0x0004
    }
    public class MapPropertyException : Exception
    {
        public MapPropertyException(string message) : base(message) { }
    }
    public class ConvertException : Exception
    {
        public ConvertException(string message) : base(message) { }
    }

}
