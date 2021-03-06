﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ucore
{
    public class JsonUtil
    {
        public static JObject ReadTextIntoJObject(string textFile)
        {
            try
            {
                // read JSON directly from a file
                using (StreamReader file = File.OpenText(textFile))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    return (JObject)JToken.ReadFrom(reader);
                }
            }
            catch (Exception e)
            {
                Log.PrintException(e, textFile);
                return null;
            }
        }

        public static int GetIntProperty(JObject obj, string propName)
        {
            if (obj.Property(propName) == null)
                return 0;

            return BaseUtil.ToInt((string)obj[propName]);
        }
    }
}
