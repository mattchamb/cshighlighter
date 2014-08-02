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
    | ParameterDeclaration of SyntaxToken
    | PropertyDeclaration of SyntaxToken
    | PropertyReference of SyntaxToken * IPropertySymbol
    | NamedTypeDeclaration of SyntaxToken * INamedTypeSymbol
    | NamedTypeReference of SyntaxToken * INamedTypeSymbol
    | MethodDeclaration of SyntaxToken
    | MethodReference of SyntaxToken * IMethodSymbol
    | EnumMemberDeclaration of SyntaxToken * IFieldSymbol
    | EnumMemberReference of SyntaxToken * IFieldSymbol
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
    | Field
    | Property
    | Method
    | Event

type TypeMember =
    {
        MemberName: string
        Signature: string
        Kind: FieldKind
    }

type TypeKind =
    | Class
    | Struct
    | Enum

type DeclaredType =
    {
        TypeName: string
        Kind: TypeKind
        Members: TypeMember list
    }

