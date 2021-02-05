using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier
{
    public partial class Printer
    {
        private Doc PrintAnonymousObjectCreationExpressionSyntax(AnonymousObjectCreationExpressionSyntax node)
        {
            return Concat(String("new"), this.PrintStatements(node.Initializers, Line, String(",")));
        }
    }
}