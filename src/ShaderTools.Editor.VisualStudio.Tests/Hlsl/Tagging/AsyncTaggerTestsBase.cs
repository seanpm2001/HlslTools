﻿using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging
{
    internal abstract class AsyncTaggerTestsBase<TTagger, TTag> : MefTestsBase
        where TTagger : AsyncTagger<TTag>
        where TTag : ITag
    {
        [TestCaseSource(typeof(VsShaderTestUtility), nameof(VsShaderTestUtility.GetTestShaders))]
        public async Task CanDoTagging(string testFile)
        {
            // Arrange.
            VisualStudioSourceTextFactory.Instance = Container.GetExportedValue<VisualStudioSourceTextFactory>();
            var sourceCode = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, testFile));
            var textBuffer = TextBufferUtility.CreateTextBuffer(Container, sourceCode);
            var backgroundParser = new BackgroundParser(textBuffer);
            var snapshot = textBuffer.CurrentSnapshot;
            var tagger = CreateTagger(backgroundParser, textBuffer);

            // Act.
            await tagger.InvalidateTags(snapshot, CancellationToken.None);
            var tagSpans = tagger.GetTags(new NormalizedSnapshotSpanCollection(new[]
            {
                new SnapshotSpan(snapshot, 0, snapshot.Length)
            })).ToList();

            // Assert.
            if (MustCreateTagSpans)
                Assert.That(tagSpans.Any());

            backgroundParser.Dispose();
        }

        protected abstract TTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer);

        protected virtual bool MustCreateTagSpans => true;
    }
}