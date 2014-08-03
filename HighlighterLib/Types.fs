module Types

open System
open System.Collections.Generic
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.Text

type TokenClassification =
    | Unformatted of SyntaxToken
    | Trivia of TriviaElement
    | Keyword of SyntaxToken
    | Identifier of SyntaxToken
    | StringLiteral of SyntaxToken
    | NumericLiteral of SyntaxToken
    | CharacterLiteral of SyntaxToken
    | LocalVariableDeclaration of SyntaxToken * ILocalSymbol
    | LocalVariableReference of SyntaxToken * ILocalSymbol
    | FieldDeclaration of SyntaxToken * IFieldSymbol
    | FieldReference of SyntaxToken * IFieldSymbol
    | ParameterReference of SyntaxToken * IParameterSymbol
    | ParameterDeclaration of SyntaxToken * IParameterSymbol
    | PropertyDeclaration of SyntaxToken * IPropertySymbol
    | PropertyReference of SyntaxToken * IPropertySymbol
    | NamedTypeDeclaration of SyntaxToken * INamedTypeSymbol
    | NamedTypeReference of SyntaxToken * INamedTypeSymbol
    | MethodDeclaration of SyntaxToken * IMethodSymbol
    | MethodReference of SyntaxToken * IMethodSymbol
    | EnumMemberDeclaration of SyntaxToken * IFieldSymbol
    | EnumMemberReference of SyntaxToken * IFieldSymbol
    | NamespaceReference of SyntaxToken * INamespaceSymbol
    | SemanticError of SyntaxToken * Diagnostic array
and TriviaElement =
    | Comment of SyntaxTrivia
    | BeginRegion of SyntaxTrivia
    | EndRegion of SyntaxTrivia
    | NewLine
    | Whitespace of SyntaxTrivia
    | DisabledText of SyntaxTrivia
    | UnformattedTrivia of SyntaxTrivia

type FieldKind =
    | Field = 1
    | Property = 2 
    | Method = 3
    | Event = 4

type TypeMember =
    {
        MemberName: string
        Signature: string
        Kind: FieldKind
    }

type TypeKind =
    | Class = 1
    | Struct = 2
    | Enum = 3

type DeclaredType =
    {
        TypeName: string
        Kind: TypeKind
        Members: TypeMember list
    }

