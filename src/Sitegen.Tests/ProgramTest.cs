using System;
using FluentAssertions;
using Sitegen.Models;
using Sitegen.Models.Config;
using Xunit;

namespace Sitegen.Tests
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
            private static readonly TopLevelConfig TopLevelConfig = Program.ReadConfig("fixtures/config_with_empty_dictionaries.yaml");

            [Fact]
            void creates_a_non_null_TopLevelConfig_Config()
            {
                Assert.NotNull(TopLevelConfig.Config);
            }

            [Fact]
            void creates_a_Config_with_the_expected_SourceDir()
            {
                Assert.Equal("src", TopLevelConfig.Config.SourceDir);
            }

            [Fact]
            void creates_a_Config_with_the_expected_LayoutsDir()
            {
                Assert.Equal("src/_layouts", TopLevelConfig.Config.LayoutsDir);
            }

            [Fact]
            void creates_a_Config_with_the_expected_OutputDir()
            {
                Assert.Equal("out", TopLevelConfig.Config.OutputDir);
            }

            [Fact]
            void creates_a_Config_with_the_expected_PostDir ()
            {
                Assert.Equal("src/_posts", TopLevelConfig.Config.PostsDir);
            }

            [Fact]
            void creates_a_Config_with_the_expected_LineBreaks_setting ()
            {
                Assert.Equal(LineBreaks.Hard, TopLevelConfig.Config.LineBreaks);
            }

            [Fact]
            void creates_a_TopLevelConfig_with_a_non_null_Site()
            {
                Assert.NotNull(TopLevelConfig.Site);
            }
        }
    }
}
