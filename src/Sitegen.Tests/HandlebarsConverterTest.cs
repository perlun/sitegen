using FluentAssertions;
using Sitegen.Extensions;
using Sitegen.Models.Config;
using Sitegen.Services;
using Xunit;

namespace Sitegen.Tests
{
    public class HandlebarsConverterTest
    {
        private static readonly HandlebarsConverter Subject;

        static HandlebarsConverterTest()
        {
            Subject = new HandlebarsConverter(new TopLevelConfig());
        }

        //
        // Handlebars helpers tests
        //

        [Fact]
        void include_includes_given_file()
        {
            string result = Subject.Convert(@"
                Content before include
                {{include 'fixtures/include.hbs'}}
                Content after include
            ".TrimLines()).Trim();

            result.Should().Be(
                "Content before include\n" +
                "Content from include file\n" +
                "\n" +                              // This extra newline comes from the newline in the {{include}} line above.
                "Content after include");
        }

        [Fact]
        void markdown_renders_inline_markdown_content()
        {
            string result = Subject.Convert(@"
                {{#markdown}}
                This is **Markdown** content. Way cool, eh?
                {{/markdown}}
            ".TrimLines()).Trim();

            result.Should().Be("<p>This is <strong>Markdown</strong> content. Way cool, eh?</p>");
        }

        [Fact]
        void set_variable_can_be_accessed_later_in_template()
        {
            string result = Subject.Convert(@"
                {{set foo='bar'}}
                Value of foo is {{foo}}
            ".TrimLines()).Trim();

            result.Should().Be("Value of foo is bar");
        }

        [Fact]
        void ifeq_executes_block_if_condition_is_truthy()
        {
            string result = Subject.Convert(@"
                {{set language='en'}}
                {{#ifeq language 'en'}}Language is English{{/ifeq}}
                {{#ifeq language 'sv'}}Language is Swedish{{/ifeq}}
            ".TrimLines()).Trim();

            result.Should().Be("Language is English");
        }
    }
}

