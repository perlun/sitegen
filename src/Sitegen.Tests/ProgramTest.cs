using System;
using FluentAssertions;
using Sitegen.Models;
using Sitegen.Models.Config;
using Xunit;

namespace Sitegen.Tests
{
    public static class ProgramTest
    {
        public static class ReadConfig
        {
            public class WithEmptyConfig
            {
                [Fact]
                void throws_expected_exception()
                {
                    Action action = () => Program.ReadConfig("fixtures/empty_config.yaml");

                    action.Should()
                        .Throw<ConfigurationException>()
                        .WithMessage("config.yaml does not contain any settings");
                }
            }

            public class WithNonExistentConfig
            {
                [Fact]
                void throws_expected_exception()
                {
                    Action action = () => Program.ReadConfig("fixtures/non_existent_config.yaml");

                    action.Should()
                        .Throw<ConfigurationException>()
                        .WithMessage("fixtures/non_existent_config.yaml does not exist");
                }
            }

            public class ConfigWithEmptyDictionaries
            {
                private static readonly TopLevelConfig TopLevelConfig =
                    Program.ReadConfig("fixtures/config_with_empty_dictionaries.yaml");

                [Fact]
                void creates_a_non_null_TopLevelConfig_Config()
                {
                    TopLevelConfig.Config.Should().NotBeNull();
                }

                [Fact]
                void creates_a_Config_with_the_expected_SourceDir()
                {
                    TopLevelConfig.Config.SourceDir
                        .Should().Be("src");
                }

                [Fact]
                void creates_a_Config_with_the_expected_LayoutsDir()
                {
                    TopLevelConfig.Config.LayoutsDir
                        .Should().Be("src/_layouts");
                }

                [Fact]
                void creates_a_Config_with_the_expected_OutputDir()
                {
                    TopLevelConfig.Config.OutputDir
                        .Should().Be("out");
                }

                [Fact]
                void creates_a_Config_with_the_expected_PostDir()
                {
                    TopLevelConfig.Config.PostsDir
                        .Should().Be("src/_posts");
                }

                [Fact]
                void creates_a_Config_with_the_expected_LineBreaks_setting()
                {
                    TopLevelConfig.Config.LineBreaks
                        .Should().Be(LineBreaks.Hard);
                }

                [Fact]
                void creates_a_TopLevelConfig_with_a_non_null_Site()
                {
                    TopLevelConfig.Site
                        .Should().NotBeNull();
                }
            }
        }
    }
}
