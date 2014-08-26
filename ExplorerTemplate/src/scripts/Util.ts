module Util {
    export function tokenClassName(tokenKind: string) {
        switch (tokenKind) {
            case "Unformatted":
                return "";
            case "Keyword":
                return "keyword";
            case "Identifier":
                return "identifier";
            case "TypeDecl":
                return "namedTypeDecl";
            case "TypeRef":
                return "namedTypeRef";
            case "StringLiteral":
                return "stringLiteral";
            case "NumericLiteral":
                return "numericLiteral";
            case "CharLiteral":
                return "characterLiteral";
            case "LocalDecl":
                return "localDecl";
            case "LocalRef":
                return "localRef";
            case "FieldDecl":
                return "fieldDecl";
            case "FieldRef":
                return "fieldRef";
            case "ParamDecl":
                return "paramDecl";
            case "ParamRef":
                return "paramRef";
            case "PropDecl":
                return "propDecl";
            case "PropRef":
                return "propRef";
            case "MethodDecl":
                return "methodDecl";
            case "MethodRef":
                return "methodRef";
            case "EnumMemberDecl":
                return "enumMemberDecl";
            case "EnumMemberRef":
                return "enumMemberRef";
            case "SemanticError":
                return "semanticError";
            case "Trivia":
                return "";
            case "Comment":
                return "comment";
            case "Region":
                return "region";
            case "DisabledText":
                return "disabled";
            default:
                return "";
        }
    };

    export function mapi(f: (idx: number, x: any) => any) {
        return function (seq) {
            var result = [];
            for (var i = 0; i < seq.length; i++) {
                result.push(f(i, seq[i]));
            }
            return result;
        };
    }

    export function map(f: (x: any) => any) {
        return function (seq) {
            var result = [];
            for (var i = 0; i < seq.length; i++) {
                result.push(f(seq[i]));
            }
            return result;
        };
    }
} 