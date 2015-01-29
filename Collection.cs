using System;
using System.Collections.Generic;
using WSD.Rest;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using PCLStorage;
using System.IO;
using System.Text;

namespace WSD.Data
{
	#pragma warning disable 0219, 0649
	public class Collection<T>
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

			CheckResponse (response);

			if (cache) {
				SetCache (key, response.Content);
			}

			return response.Get<List<T>> ();
		}

		public static async Task<S> CallGet<S>(string api, Query query = null)
		{
			Response response = await Client.Get (api, query);

			CheckResponse (response);

			return response.Get<S> ();
		}

		public static async Task<Response> CallGet(string api, Query query = null)
		{
			Response response = await Client.Get (api, query);

			CheckResponse (response);

			return response;
		}

		public static async Task<S> CallPost<S>(string api, string id, Dictionary<string, object> data)
		{
			Response response = await Client.Post (api, id, data);

			CheckResponse (response);

			return response.Get<S> ();
		}

		public static async Task<Response> CallPost(string api, string id, Dictionary<string, object> data)
		{
			Response response = await Client.Post (api, id, data);

			CheckResponse (response);

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

			CheckResponse (response);

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

			CheckResponse (response);

			Id = response.GetString ("id");

			return response.Get<T> ();
		}

		public Dictionary<string, object> GetProperties()
		{
			return ObjectHelper.GetProperties (this);
		}

		public Dictionary<string, S> GetProperties<S>()
		{
			return ObjectHelper.GetProperties<S> (this);
		}

		public async void Delete()
		{
			Response response = await Client.Delete (
				string.Format("{0}/{1}", CollectionPath, CollectionName()),
				Id
			);

			CheckResponse (response);
		}

		static void CheckResponse (Response response)
		{
			if (response == null) {
				throw new Exception("Unkown exception: response was null");
			} else if (response.Content == null) {
				throw new Exception("Unkown exception: response content was null");
			} else if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception(string.Format(
					"Exception: ({0}) {1}", 
					response.GetErrorCode(), 
					response.GetErrorMessage ()
				));
			}
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

