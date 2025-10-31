using System.Reflection;
using System.Reflection.Emit;

namespace MonthlyScheduler.Utilities;

public static class RuntimeTypeBuilder
{
    private static AssemblyBuilder? assemblyBuilder;
    private static ModuleBuilder? moduleBuilder;
    private static int typeCount;

    private static void EnsureInitialized()
    {
        if (assemblyBuilder == null)
        {
            var assemblyName = new AssemblyName("DynamicTypes");
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicTypesModule");
        }
    }

    public static Type GetDynamicType(Dictionary<string, object> properties)
    {
        EnsureInitialized();

        typeCount++;
        var typeBuilder = moduleBuilder!.DefineType($"DynamicType{typeCount}", 
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | 
            TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | 
            TypeAttributes.AutoLayout);

        foreach (var prop in properties)
        {
            var propertyType = prop.Value is Type ? (Type)prop.Value : prop.Value.GetType();
            var fieldBuilder = typeBuilder.DefineField("_" + prop.Key, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(prop.Key, PropertyAttributes.HasDefault, propertyType, null);

            var getMethodBuilder = typeBuilder.DefineMethod("get_" + prop.Key,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);

            var setMethodBuilder = typeBuilder.DefineMethod("set_" + prop.Key,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null, new[] { propertyType });

            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            var setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        return typeBuilder.CreateType()!;
    }
}