# README

## Examples of sites using `sitegen`

- https://github.com/perlun/halleluja.nu

## Building sitegen

```shell
$ dotnet build
```

## `config.yaml` format

TODO: document this

## Variables available in Handlebars files

- `now` - the current `DateTime`, as a .NET DateTime object.
- `site` - the `site` section in `config.yaml`
- `blog_posts` - a list of all blog posts.

## Similar projects

See also the following C#/.NET-based static site generators:

 - https://github.com/krompaco/record-collector - built on top of ASP.NET Core MVC
 - https://github.com/tomzorz/lastpage - .NET Core static website generator based on Mustachio

## License

[MIT](LICENSE)
