# README

## Examples of sites using `sitegen`

- https://github.com/perlun/halleluja.nu

## Building sitegen

```shell
$ dotnet build
```

## `config.yaml` format

```yaml
config:
  source_dir: src
  layouts_dir: src/_layouts
  output_dir: out
  posts_dir: src/_posts
  multiple_languages: true

site:
  title: halleluja.nu
```

See the [TopLevelConfig](SiteGenerator.ConsoleApp/Models/Config/TopLevelConfig.cs) class for details. YAML keys are converted from `snake_case_` to `PascalCase`, so `source_dir` in the `config.yaml` corresponds to the `SourceDir` property in [TopLevelConfig](SiteGenerator.ConsoleApp/Models/Config/Config.cs) and so forth.

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
