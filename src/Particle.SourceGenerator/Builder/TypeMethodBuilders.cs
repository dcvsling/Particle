using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator.Builder;

public static class TypeMethodBuilders
{
    public static TypeDeclarationSyntax CreatePartialType(string kind, string identifier, MemberDeclarationSyntax[] members, BaseListSyntax? baseList = null)
    {
        var id = Identifier(identifier);
        TypeDeclarationSyntax decl = kind == "struct" ? StructDeclaration(id) : ClassDeclaration(id);
        decl = decl.AddModifiers(Token(SyntaxKind.PartialKeyword));
        if (baseList is not null) decl = decl.WithBaseList(baseList);
        return decl.WithMembers(List(members));
    }
}
