using System;
using System.Collections.Generic;
using WSD.Rest;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using PCLStorage;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

namespace WSD.Data
{
	#pragma warning disable 0219, 0649
	public class Collection<T> : ICollection
	{
		public string Id { get; set; }
		public static string FieldId = "id";
		public static string CollectionPath = "";

		static string CollectionName ()
		{
			return typeof(T).Name.Pluralize ().ToLower ();
		}

		public static async Task<List<T>> Find(Query query, bool cache = false, bool reloadCache = false)
		{
			Response response = null;
			string key = query == null ? string.Format ("{0}::Find", CollectionName ()) : query.ToString ();

			if (cache && reloadCache == false) {
				string content = await GetCache (key);

				if (content != null) {
					response = new Response ();
					response.Content = content;
					return response.Get<List<T>> ();
				}
			}

			response = await Client.Get (
				string.Format("{0}/{1}", CollectionPath, CollectionName()), 
				query
			);

			response.Check ();

			if (cache) {
				SetCache (key, response.Content);
			}

			return response.Get<List<T>> ();
		}

		public static async Task<S> CallGet<S>(string api, Query query = null)
		{
			Response response = await Client.Get (api, query);

			response.Check ();

			return response.Get<S> ();
		}

		public static async Task<Response> CallGet(string api, Query query = null)
		{
			Response response = await Client.Get (api, query);

			response.Check ();

			return response;
		}

		public static async Task<S> CallPost<S>(string api, string id = null, Dictionary<string, object> data = null)
		{
			Response response = await Client.Post (api, id, data);

			response.Check ();

			return response.Get<S> ();
		}

		public static async Task<Response> CallPost(string api, string id = null, Dictionary<string, object> data = null)
		{
			Response response = await Client.Post (api, id, data);

			response.Check ();

			return response;
		}

		public static async Task<T> FindById(string id, bool cache = false, bool reloadCache = false)
		{
			Response response = null;
			string key = string.Format ("{0}::FindById({1})", CollectionName (), id);

			if (cache && reloadCache == false) {
				string content = await GetCache (key);

				if (content != null) {
					response = new Response ();
					response.Content = content;
					return response.Get<T> ();
				}
			}

			response = await Client.Get (
				string.Format("{0}/{1}/{2}", CollectionPath, CollectionName(), id)
			);

			response.Check ();

			if (cache) {
				SetCache (key, response.Content);
			}

			return response.Get<T> ();
		}

		public virtual async Task<T> Save()
		{
			Response response = null;
			string api = "";

			if (CollectionPath != null && CollectionPath.Length > 0) {
				api += string.Format ("/{0}", CollectionPath);
			}

			api += string.Format ("/{0}", CollectionName ());

			Dictionary<string, object> data = GetProperties ();

			response = await Client.Post(api, Id, data);

			response.Check ();

			Id = response.GetString ("id");

			return response.Get<T> ();
		}

		public async Task<bool> Delete()
		{
			Response response = await Client.Delete (
				string.Format("{0}/{1}", CollectionPath, CollectionName()),
				Id
			);

			response.Check ();

			return true;
		}

		public Dictionary<string, object> GetProperties (object obj = null)
		{
			if (obj == null) obj = this;

			IEnumerable<PropertyInfo> properties = obj.GetType ().GetRuntimeProperties ();

			Dictionary<string, object> data = new Dictionary<string, object> ();

			var enumerator = properties.GetEnumerator ();

			while (enumerator.MoveNext ()) {
				PropertyInfo property = enumerator.Current;
				object value = property.GetValue (obj);

				if (value is IList) {
					List<object> _data = new List<object> ();

					foreach (object _obj in value as IList) {
						if (_obj is File) {
							_data.Add (_obj);
						} else {
							_data.Add (GetProperties (_obj));
						}
					}

					data.Add (property.Name, _data);
				} else if (value is IDictionary) {
					Dictionary<string, object> _data = new Dictionary<string, object> ();

					foreach (KeyValuePair<string, object> _obj in value as IDictionary) {
						if (_obj.Value is File) {
							_data.Add (_obj.Key, _obj.Value);
						} else {
							_data.Add (_obj.Key, GetProperties (_obj.Value));
						}
					}

					data.Add (property.Name, _data);
				} else if (value is WSD.Data.ICollection) {
					data.Add (property.Name, GetProperties (value));
				} else {
					data.Add (property.Name, value);
				}
			}

			return data;
		}

		static private async void SetCache (string key, string content)
		{
			IFile file = await Cache (key);
			await file.WriteAllTextAsync (content);
		}

		static private async Task<string> GetCache (string key)
		{
			IFile file = await Cache (key);
			return await file.ReadAllTextAsync();
		}

		static private async Task<IFile> Cache(string fileName)
		{
			byte[] bytes = Encoding.UTF8.GetBytes (fileName);
			fileName = Convert.ToBase64String (bytes);

			foreach (char c in Path.GetInvalidFileNameChars ()) {
				fileName = fileName.Replace (c.ToString (), "");
			}

			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = await rootFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
			IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

			return file;
		}
	}
	#pragma warning restore 0219, 0649
}

