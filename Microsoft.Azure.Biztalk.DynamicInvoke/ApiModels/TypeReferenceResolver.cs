//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Biztalk.DynamicInvoke.SwaggerParsers;
    
    /// <summary>
    /// Helper class to resolve type references
    /// </summary>
    public class TypeReferenceResolver
    {
        private readonly Dictionary<string, DataType> typesMap;

        private TypeReferenceResolver(IEnumerable<DataType> types)
        {
            typesMap = types.ToDictionary(t => t.Name);
        }

        /// <summary>
        /// Go through the provided set of types,
        /// and replace any type references with
        /// corresponding DataType objects. Returns
        /// a new set of fixed-up DataType objects.
        /// </summary>
        /// <param name="types">The data types to fix up, treated as a set.</param>
        /// <returns>The new resolved types, not necessarily in the same order.</returns>
        public static IEnumerable<DataType> ResolveTypeReferences(IEnumerable<DataType> types)
        {
            var resolver = new TypeReferenceResolver(types);
            return resolver.FixUpTypes();
        }

        public static IEnumerable<Operation> ResolveOperationReferences(IEnumerable<DataType> types,
            IEnumerable<Operation> operations)
        {
            var resolver = new TypeReferenceResolver(types);
            return resolver.FixUpOperations(operations);
        } 

        private IEnumerable<DataType> FixUpTypes()
        {
            var originalTypes = typesMap.Values.ToList();
            return originalTypes.Select(ResolveType).ToList();
        }

        private DataType ResolveType(DataType t)
        {
            if (t is PrimitiveDataType)
            {
                // nothing to do, we're good!
                return t;
            }

            var reference = t as TypeReferenceDataType;
            if (reference != null)
            {
                DataType resolvedType = ResolveType(typesMap[Swagger20Parser.StripDefinitionPrefix(t.Name)]);
                typesMap[Swagger20Parser.StripDefinitionPrefix(t.Name)] = resolvedType;
                return resolvedType;
            }

            var compositeType = t as CompositeDataType;
            if (compositeType != null)
            {
                if (compositeType.Properties.All(p => !(p.Value is TypeReferenceDataType)))
                {
                    // No type refs, we're good!
                    return compositeType;
                }

                var resolvedType = new CompositeDataType(compositeType.Name,
                    compositeType.Properties.Select(p => 
                    {
                        if (p.Value is TypeReferenceDataType && p.Value.Name == compositeType.Name)
                        {
                            return new KeyValuePair<string, DataType>(p.Key, new SelfReferenceDataType());
                        }

                        return new KeyValuePair<string, DataType>(
                            p.Key, ResolveType(p.Value));
                    }), compositeType.Required);
                typesMap[resolvedType.Name] = resolvedType;
                return resolvedType;
            }

            var arrayType = t as ArrayDataType;
            if (arrayType != null)
            {
                if (arrayType.ItemType is TypeReferenceDataType)
                {
                    return new ArrayDataType(ResolveType(arrayType.ItemType));
                }

                return t;
            }

            var mapType = t as MapDataType;
            if (mapType != null)
            {
                if (mapType.AdditionalPropertiesType is TypeReferenceDataType)
                {
                    return new MapDataType(ResolveType(mapType.AdditionalPropertiesType));
                }

                return t;
            }

            throw new InvalidOperationException("Unknown data type, coding error!");
        }

        private IEnumerable<Operation> FixUpOperations(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                if (!ContainsTypeRef(operation))
                {
                    yield return operation;
                }
                else
                {
                    yield return new Operation(operation.Name, operation.ReturnType, operation.UriTemplate, operation.Method, operation.Authorization,
                        operation.Parameters.Select(ResolveParameter), operation.ExpectedStatusCodes, operation.Ref);
                }
            }
        }

        private bool ContainsTypeRef(Operation operation)
        {
            return operation.Parameters.Any(p => typesMap.ContainsKey(p.Type));
        }

        private IOperationParameter ResolveParameter(IOperationParameter parameter)
        {
            if (!typesMap.ContainsKey(parameter.Type))
            {
                return parameter;
            }

            if (parameter is BodyOperationParameter)
            {
                return new BodyOperationParameter(typesMap[parameter.Type], parameter.IsRequired);
            }

            throw new InvalidOperationException("Type reference in non-body operation parameter, possible invalid swagger");
        }
    }
}
