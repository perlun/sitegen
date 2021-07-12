module Test

open Sitegen
open Sitegen.Models
open Xunit

type ReadConfig() =
    let topLevelConfig = Program.ReadConfig("fixtures/empty_config.yaml")

    [<Fact>]
    let ``creates a non-null TopLevelConfig.Config`` () =
        Assert.NotNull(topLevelConfig.Config)

    [<Fact>]
    let ``creates a Config with the expected SourceDir`` () =
        Assert.Equal("src", topLevelConfig.Config.SourceDir)

    [<Fact>]
    let ``creates a Config with the expected LayoutsDir`` () =
        Assert.Equal("src/_layouts", topLevelConfig.Config.LayoutsDir)

    [<Fact>]
    let ``creates a Config with the expected OutputDir`` () =
        Assert.Equal("out", topLevelConfig.Config.OutputDir)

    [<Fact>]
    let ``creates a Config with the expected PostDir`` () =
        Assert.Equal("src/_posts", topLevelConfig.Config.PostsDir)

    [<Fact>]
    let ``creates a Config with the expected LineBreaks setting`` () =
        Assert.Equal(LineBreaks.Hard, topLevelConfig.Config.LineBreaks.Value)

    [<Fact>]
    let ``creates a TopLevelConfig with a null Site`` () =
        Assert.Null(topLevelConfig.Site)
