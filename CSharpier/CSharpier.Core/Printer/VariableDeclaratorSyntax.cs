using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core
{
    public partial class Printer
    {
        private Doc PrintVariableDeclaratorSyntax(VariableDeclaratorSyntax node)
        {
            var parts = new Parts();
            parts.Add(node.Identifier.Text);
            if (node.ArgumentList != null) {
                parts.Add(this.PrintBracketedArgumentListSyntax(node.ArgumentList));
            }
            if (node.Initializer != null) {
                parts.Add(this.PrintEqualsValueClauseSyntax(node.Initializer));
            }
            return Concat(parts);
        }
    }
}