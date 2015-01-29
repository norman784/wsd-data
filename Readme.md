WSD.Data
---

Simple library to work with api servers in ActiveRecord way, built on top of [WSD.Rest](http://github.com/norman784/wsd-rest)

Setup
---

To setup we need to initialize the lib:

```
Client.Setup ("http://our.production.url", "http://our.dev.url", "/v1", "x-application-id", "x-application-secret", "x-access-token");
Client.Init ("application id", "application secret");
```

**Client.Setup** (params)
- Production URL
- Development URL, yes it detects when you are using Debug or Release your app
- API version
- Header for application id, is used when you has one server for more than one app, in the case that you are building a BaaS or something else
- Header for application secret
- Header for application access token, when you app works with authentication

**Client.Init** (params)
- Application id
- Application secret

Usage
---

**Find**

```
public class Post : Collection<Post>
{
	public string Title { get; set; }
	public string Content { get; set; }
	public File Image { get; set; }
}

// Just a common query, this will be converted to querystring
Query query = new Query ();
query.Search = "A search them";
query.Offset = 0;
query.Limit = 50;
query.Order = "id desc";

List<Post> posts =  await Post.Find(query);
```

**FindById**

```
Post post = await Post.FindById("1");
```

**Save**

```
byte[] fileContents = ....; // it must be implemented in each platform no PCL way yet

Post post = new Post();
post.Title = "Post title";
post.Content = "Post content";
post.Image = new File ("FileName.png", "image/png", fileContents);
await post.Save();
```

**Delete**

```
post.Delete ();
```

**Custom Calls**

You can extend it to put your custom calls

```
public class Post : Collection<Post>
{
	public string Title { get; set; }
	public string Content { get; set; }
	public File Image { get; set; }

	static public async Post FindByTag (string tags)
	{
		TagQuery query = new TagQuery ();
		query.tags = tags;

		return await CallGet<List<Post>> ("/posts_by_tags", query);
	}
}
```

Todo
---

Better documentation