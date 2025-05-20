using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace Harris.GPC
{
	public class ObjectSerializer
	{
		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializableObject"></param>
		/// <param name="fileName"></param>
		public static void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) {return; }

			try
			{
				Console.WriteLine(serializableObject.GetType());
				XmlDocument xmlDocument = new XmlDocument();
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				/*using (MemoryStream stream = new MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);

					//TextWriter writer = new StreamWriter(fileName);
					//serializer.Serialize(writer, serializableObject);

					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
				}*/

				FileStream writer = new(fileName, FileMode.Create);
				serializer.Serialize(writer, serializableObject);
				writer.Close();
			}
			catch (Exception ex)
			{
				//Log exception here
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Deserializes an xml file into an object list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static T DeSerializeObject<T>(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) { return default(T); }

			T objectOut = default(T);

			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(fileName);
				string xmlString = xmlDocument.OuterXml;

				using (StringReader read = new StringReader(xmlString))
				{
					Type outType = typeof(T);

					XmlSerializer serializer = new XmlSerializer(outType);
					using (XmlReader reader = new XmlTextReader(read))
					{
						objectOut = (T)serializer.Deserialize(reader);
					}
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}

			return objectOut;
		}
	}
}
