﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.DocumentationRules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// A <c>&lt;param&gt;</c> tag within a C# element's documentation header is empty.
    /// </summary>
    /// <remarks>
    /// <para>C# syntax provides a mechanism for inserting documentation for classes and elements directly into the
    /// code, through the use of XML documentation headers. For an introduction to these headers and a description of
    /// the header syntax, see the following article:
    /// <see href="http://msdn.microsoft.com/en-us/magazine/cc302121.aspx">XML Comments Let You Build Documentation
    /// Directly From Your Visual Studio .NET Source Files</see>.</para>
    ///
    /// <para>A violation of this rule occurs if the documentation for an element contains a <c>&lt;param&gt;</c> tag
    /// which is empty and does not contain a description of the parameter.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoCodeFix("Cannot generate documentation")]
    internal class SA1614ElementParameterDocumentationMustHaveText : ElementDocumentationBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1614ElementParameterDocumentationMustHaveText"/>
        /// analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1614";
        private const string Title = "Element parameter documentation must have text";
        private const string MessageFormat = "Element parameter documentation must have text";
        private const string Description = "A <param> tag within a C# element's documentation header is empty.";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1614.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.DocumentationRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        public SA1614ElementParameterDocumentationMustHaveText()
            : base(matchElementName: XmlCommentHelper.ParamXmlTag, inheritDocSuppressesWarnings: true)
        {
        }

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        protected override void HandleXmlElement(SyntaxNodeAnalysisContext context, IEnumerable<XmlNodeSyntax> syntaxList, params Location[] diagnosticLocations)
        {
            var xmlParameterNames = syntaxList
                .Where(x => string.Equals(x.GetName()?.ToString(), XmlCommentHelper.ParamXmlTag))
                .Select(x =>
                {
                    bool isEmpty = x is XmlEmptyElementSyntax || XmlCommentHelper.IsConsideredEmpty(x);
                    var location = x.GetLocation();

                    return new Tuple<bool, Location>(isEmpty, location);
                })
                .ToImmutableArray();

            VerifyParameters(context, xmlParameterNames, diagnosticLocations.First());
        }

        /// <inheritdoc/>
        protected override void HandleCompleteDocumentation(SyntaxNodeAnalysisContext context, XElement completeDocumentation, params Location[] diagnosticLocations)
        {
            var xmlParameterNames = completeDocumentation.Nodes()
                .OfType<XElement>()
                .Where(e => e.Name == XmlCommentHelper.ParamXmlTag)
                .Select(x =>
                {
                    return new Tuple<bool, Location>(XmlCommentHelper.IsConsideredEmpty(x), null);
                })
                .ToImmutableArray();

            VerifyParameters(context, xmlParameterNames, diagnosticLocations.First());
        }

        private static void VerifyParameters(SyntaxNodeAnalysisContext context, ImmutableArray<Tuple<bool, Location>> documentationParameters, Location identifierLocation)
        {
            var index = 0;

            foreach (var documentedParameter in documentationParameters)
            {
                if (documentedParameter.Item1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, documentedParameter.Item2 ?? identifierLocation));
                }

                index++;
            }
        }
    }
}
