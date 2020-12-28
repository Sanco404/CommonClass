using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TF_Img_Utils
{
   public static  class DataTableHelper<T> where T:new()
    {
       public static List<T> DataTableToLstModel(DataTable dt)
       {
           // 定义的集合    
           List<T> ts = new List<T>();
           // 获得此模型的类型   
           Type type = typeof(T);
           string tempName = "";


           foreach (DataRow dr in dt.Rows)
           {
               T t = new T();

               FieldInfo[] fields = t.GetType().GetFields();
               foreach (FieldInfo finfo in fields)
               {
                    tempName = finfo.Name;  // 检查DataTable是否包含此列    
                   if (dt.Columns.Contains(tempName))
                   {
                       object value = dr[tempName];
                       if (value != DBNull.Value)
                           finfo.SetValue(t,value);
                   }
               }

               // 获得此模型的公共属性      
               PropertyInfo[] propertys = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
               foreach (PropertyInfo pi in propertys)
               {
                   tempName = pi.Name;  // 检查DataTable是否包含此列    


                   if (dt.Columns.Contains(tempName))
                   {
                       // 判断此属性是否有Setter      
                       if (!pi.CanWrite) continue;


                       object value = dr[tempName];
                       if (value != DBNull.Value)
                           pi.SetValue(t, value, null);
                   }
               }
               ts.Add(t);
           }
           return ts;
       }

    }
}
