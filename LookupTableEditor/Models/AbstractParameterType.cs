﻿using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LookupTableEditor
{
    public partial class AbstractParameterType : ObservableObject
    {
        public static AbstractParameterType Empty() => new((FamilyParameter)null);

        [ObservableProperty]
        private string? _sizeTablesTypeName = string.Empty;

#if R22_OR_GREATER
        public static AbstractParameterType FromDefinitionOfParameterType(
            DefinitionOfParameterType def
        )
        {
            var param = new AbstractParameterType(new ForgeTypeId(def.TypeName));
            param.SizeTablesTypeName = def.SizeTableType;
            return param;
        }

        public static List<AbstractParameterType> GetAllTypes()
        {
            var type = typeof(SpecTypeId);
            var typesProps = type.GetProperties().ToList();
            typesProps.AddRange(type.GetNestedTypes().SelectMany(t => t.GetProperties()));
            return typesProps
                .Where(p => p.Name != nameof(SpecTypeId.Custom))
                .Select(p => new AbstractParameterType((ForgeTypeId)p.GetValue(null)))
                .ToList();
        }

        public ForgeTypeId? ParameterType { get; }
        public string Label
        {
            get
            {
                var discpline = UnitUtils.IsMeasurableSpec(ParameterType)
                    ? LabelUtils.GetLabelForDiscipline(UnitUtils.GetDiscipline(ParameterType))
                        + " : "
                    : string.Empty;
                return discpline + LabelUtils.GetLabelForSpec(ParameterType);
            }
        }

        public AbstractParameterType(ForgeTypeId? parameterType)
        {
            ParameterType = parameterType;
        }

        public AbstractParameterType(FamilyParameter? parameter)
        {
            ParameterType = parameter?.Definition.GetDataType();
        }

        public override bool Equals(object obj) => ToString().Equals(obj.ToString());

        public override int GetHashCode() => ToString().GetHashCode();

        public override string ToString() =>
            ParameterType != null ? ParameterType.TypeId.Split('-').First() : string.Empty;
#else
        public static AbstractParameterType FromDefinitionOfParameterType(
            DefinitionOfParameterType def
        )
        {
            Enum.TryParse<UnitType>(def.TypeName, out var type);
            var param = new AbstractParameterType(type);
            param.SizeTablesTypeName = def.SizeTableType;
            return param;
        }

        public static List<AbstractParameterType> GetAllTypes()
        {
            var type = typeof(UnitType);
            return Enum.GetValues(type)
                .Cast<UnitType>()
                .Select(p => new AbstractParameterType(p))
                .ToList();
        }

        public UnitType? UnitType { get; }

        public AbstractParameterType(UnitType? unitType)
        {
            UnitType = unitType;
        }

        public AbstractParameterType(FamilyParameter? parameter)
        {
            UnitType = parameter?.Definition.UnitType;
        }

        public string Label =>
            UnitType.HasValue ? LabelUtils.GetLabelFor((ParameterType)UnitType) : " - ";

        public override string ToString() => UnitType.HasValue ? UnitType.ToString() : string.Empty;

#endif
    }
}
