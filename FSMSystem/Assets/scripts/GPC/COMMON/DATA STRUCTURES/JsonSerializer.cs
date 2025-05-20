using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Harris.GPC
{
	public class JsonSerializer
	{
        public static void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) {return; }

            string json = JsonUtility.ToJson(serializableObject);

            Debug.Log("Serialized object: " + json);
        }
    }
}