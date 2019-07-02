using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynTypeMetadata : ITypeMetadata
    {
        private readonly ITypeSymbol _symbol;
        private readonly bool _isNullable;
        private readonly bool _isTask;

        public RoslynTypeMetadata(ITypeSymbol symbol, bool isNullable, bool isTask)
        {
            _symbol = symbol;
            _isNullable = isNullable;
            _isTask = isTask;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.GetName() + (IsNullable? "?" : string.Empty);
        public string FullName => _symbol.GetFullName() + (IsNullable? "?" : string.Empty);
        public bool IsAbstract => (_symbol as INamedTypeSymbol)?.IsAbstract ?? false;
        public bool IsGeneric => (_symbol as INamedTypeSymbol)?.TypeParameters.Any() ?? false;
        public bool IsDefined => _symbol.Locations.Any(l => l.IsInSource);
        public bool IsValueTuple => _symbol.Name == "" && _symbol.BaseType?.Name == "ValueType" && _symbol.BaseType.ContainingNamespace.Name == "System";

        public string Namespace => _symbol.GetNamespace();
        public ITypeMetadata Type => this;

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public IClassMetadata BaseClass => RoslynClassMetadata.FromNamedTypeSymbol(_symbol.BaseType);
        public IClassMetadata ContainingClass => RoslynClassMetadata.FromNamedTypeSymbol(_symbol.ContainingType);
        public IEnumerable<IConstantMetadata> Constants => RoslynConstantMetadata.FromFieldSymbols(_symbol.GetMembers().OfType<IFieldSymbol>());
        public IEnumerable<IDelegateMetadata> Delegates => RoslynDelegateMetadata.FromNamedTypeSymbols(_symbol.GetMembers().OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Delegate));
        public IEnumerable<IEventMetadata> Events => RoslynEventMetadata.FromEventSymbols(_symbol.GetMembers().OfType<IEventSymbol>());
        public IEnumerable<IFieldMetadata> Fields => RoslynFieldMetadata.FromFieldSymbols(_symbol.GetMembers().OfType<IFieldSymbol>());
        public IEnumerable<IInterfaceMetadata> Interfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(_symbol.Interfaces);
        public IEnumerable<IMethodMetadata> Methods => RoslynMethodMetadata.FromMethodSymbols(_symbol.GetMembers().OfType<IMethodSymbol>());
        public IEnumerable<IPropertyMetadata> Properties => RoslynPropertyMetadata.FromPropertySymbol(_symbol.GetMembers().OfType<IPropertySymbol>());
        public IEnumerable<IClassMetadata> NestedClasses => RoslynClassMetadata.FromNamedTypeSymbols(_symbol.GetMembers().OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Class));
        public IEnumerable<IEnumMetadata> NestedEnums => RoslynEnumMetadata.FromNamedTypeSymbols(_symbol.GetMembers().OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Enum));
        public IEnumerable<IInterfaceMetadata> NestedInterfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(_symbol.GetMembers().OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Interface));
        public IEnumerable<IFieldMetadata> TupleElements
        {
            get
            {
                try
                {
                    if (_symbol is INamedTypeSymbol n)
                    {
                        if (n.Name == "" && n.BaseType?.Name == "ValueType" && n.BaseType.ContainingNamespace.Name == "System")
                        {
                            var property = n.GetType().GetProperty(nameof(TupleElements));
                            if (property != null)
                            {
                                var value = property.GetValue(_symbol);
                                var tupleElements = value as IEnumerable<IFieldSymbol>;

                                return RoslynFieldMetadata.FromFieldSymbols(tupleElements);
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return new IFieldMetadata[0];
            }
        }

        public IEnumerable<ITypeMetadata> TypeArguments
        {
            get
            {
                if (_symbol is INamedTypeSymbol namedTypeSymbol)
                    return FromTypeSymbols(namedTypeSymbol.TypeArguments);

                if (_symbol is IArrayTypeSymbol arrayTypeSymbol)
                    return FromTypeSymbols(new [] { arrayTypeSymbol.ElementType});

                return new ITypeMetadata[0];
            }
        }

        public IEnumerable<ITypeParameterMetadata> TypeParameters
        {
            get
            {
                if (_symbol is INamedTypeSymbol namedTypeSymbol)
                    return RoslynTypeParameterMetadata.FromTypeParameterSymbols(namedTypeSymbol.TypeParameters);

                return new ITypeParameterMetadata[0];
            }
        }

        public bool IsEnum => _symbol.TypeKind == TypeKind.Enum;
        public bool IsEnumerable => _symbol.ToDisplayString() != "string" && (
            _symbol.TypeKind == TypeKind.Array ||
            _symbol.ToDisplayString() == "System.Collections.IEnumerable" ||
            _symbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.Collections.IEnumerable"));
        public bool IsNullable => _isNullable;
        public bool IsTask => _isTask;

        public static ITypeMetadata FromTypeSymbol(ITypeSymbol symbol)
        {
            if (symbol.Name == "Nullable" && symbol.ContainingNamespace.Name == "System")
            {
                var type = symbol as INamedTypeSymbol;
                var argument = type?.TypeArguments.FirstOrDefault();

                if (argument != null)
                    return new RoslynTypeMetadata(argument, true, false);
            }
            else if (symbol.Name == "Task" && symbol.ContainingNamespace.GetFullName() == "System.Threading.Tasks")
            {
                var type = symbol as INamedTypeSymbol;
                var argument = type?.TypeArguments.FirstOrDefault();

                if (argument != null)
                {
                    if (argument.Name == "Nullable" && argument.ContainingNamespace.Name == "System")
                    {
                        type = argument as INamedTypeSymbol;
                        var innerArgument = type?.TypeArguments.FirstOrDefault();

                        if (innerArgument != null)
                            return new RoslynTypeMetadata(innerArgument, true, true);
                    }

                    return new RoslynTypeMetadata(argument, false, true);
                }

                return new RoslynVoidTaskMetadata();
            }

            return new RoslynTypeMetadata(symbol,  !symbol.IsReferenceType, false);
        }

        public static IEnumerable<ITypeMetadata> FromTypeSymbols(IEnumerable<ITypeSymbol> symbols)
        {
            return symbols.Select(FromTypeSymbol);
        }
    }

    public class RoslynVoidTaskMetadata : ITypeMetadata
    {
        public string DocComment => null;
        public string Name => "Void";
        public string FullName => "System.Void";
        public bool IsAbstract => false;
        public bool IsEnum => false;
        public bool IsEnumerable => false;
        public bool IsGeneric => false;
        public bool IsNullable => false;
        public bool IsTask => true;
        public bool IsDefined => false;
        public bool IsValueTuple => false;
        public string Namespace => "System";
        public ITypeMetadata Type => null;

        public IEnumerable<IAttributeMetadata> Attributes => new IAttributeMetadata[0];
        public IClassMetadata BaseClass => null;
        public IClassMetadata ContainingClass => null;
        public IEnumerable<IConstantMetadata> Constants => new IConstantMetadata[0];
        public IEnumerable<IDelegateMetadata> Delegates => new IDelegateMetadata[0];
        public IEnumerable<IEventMetadata> Events => new IEventMetadata[0];
        public IEnumerable<IFieldMetadata> Fields => new IFieldMetadata[0];
        public IEnumerable<IInterfaceMetadata> Interfaces => new IInterfaceMetadata[0];
        public IEnumerable<IMethodMetadata> Methods => new IMethodMetadata[0];
        public IEnumerable<IPropertyMetadata> Properties => new IPropertyMetadata[0];
        public IEnumerable<IClassMetadata> NestedClasses => new IClassMetadata[0];
        public IEnumerable<IEnumMetadata> NestedEnums => new IEnumMetadata[0];
        public IEnumerable<IInterfaceMetadata> NestedInterfaces => new IInterfaceMetadata[0];
        public IEnumerable<ITypeMetadata> TypeArguments => new ITypeMetadata[0];
        public IEnumerable<ITypeParameterMetadata> TypeParameters => new ITypeParameterMetadata[0];
        public IEnumerable<IFieldMetadata> TupleElements => new IFieldMetadata[0];
    }
}