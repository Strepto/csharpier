using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier
{
    public partial class Printer
    {
        private Doc PrintRefTypeExpressionSyntax(RefTypeExpressionSyntax node)
        {
            return Concat(
                node.Keyword.Text,
                "(",
                this.Print(node.Expression),
                ")"
            );
        }
    }
}