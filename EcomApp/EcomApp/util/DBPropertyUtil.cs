using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace EcomApp.util
{
    public static class DBPropertyUtil
    {
        public static string GetConnectionString(string propertyFileName)
        {
            return ConfigurationManager.ConnectionStrings["EcomAppDB"].ConnectionString;
                       
        }
       

    }
}
