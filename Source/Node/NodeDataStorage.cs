///////////////////////////////////////////////////////
/// Filename: NodeDataStorage.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.IO;
using EppNet.Node;
using EppNet.Settings;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Sandboxed storage for each <see cref="NetworkNode"/>
/// </summary>

public sealed class NodeDataStorage : INodeDescendant, IDisposable
{

    public static List<Type> SupportedCollectionTypes =>
        new()
        {
            typeof(List<>),
            typeof(HashSet<>),
            typeof(SortedSet<>),
            typeof(LinkedList<>)
        };

    public static bool IsSupportedCollectionType(Type type)
    {
        foreach (Type collType in SupportedCollectionTypes)
        {
            if (type == collType)
                return true;
        }

        return false;
    }

    public NetworkNode Node { get; }

    public IReadOnlyDictionary<ushort, Type> ResolverId2Types => _resolverId2Type;
    public IReadOnlyDictionary<Type, IResolver> ResolverType2Classes => _resolverType2Class;

    public BytePayloadPool PayloadPool { get; }
    public Configuration Configuration { get; }

    private readonly Dictionary<ushort, Type> _resolverId2Type;
    private readonly Dictionary<Type, IResolver> _resolverType2Class;

    /// <summary>
    /// A map of custom data stored by <see cref="IDataHolder"/>s
    /// </summary>
    private readonly ConditionalWeakTable<IDataHolder, Dictionary<string, object>> _customDataMap;

    internal NodeDataStorage(NetworkNode node)
    {
        this.Node = node ?? throw new ArgumentNullException(nameof(node));
        this.Configuration = new(node);
        this.PayloadPool = new(node, defaultCapacity: 64);

        this._resolverId2Type = new();
        this._resolverType2Class = new();
        this._customDataMap = new();
    }

    /// <summary>
    /// Tries to register an <see cref="IResolver"/> with the specified ushort ID
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="resolver"></param>
    /// <returns>Whether or not the </returns>

    public bool RegisterResolver<T>(ushort id, IResolver<T> resolver)
    {
        Guard.AgainstNull(resolver, "Cannot register a null resolver!");
        Type type = typeof(T);
        try
        {
            this._resolverId2Type.Add(id, type);
            this._resolverType2Class.Add(type, resolver);
            return true;
        }
        catch (Exception _) { }

        return false;
    }

    /// <summary>
    /// Tries to fetch the <see cref="IResolver"/> class for the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resolver"></param>
    /// <returns>The located <see cref="IResolver"/> or null</returns>

    public bool TryGetResolver(Type type, out IResolver resolver)
    {
        if (type.IsArray)
            type = type.GetElementType();

        else if (type.IsEnum)
            type = type.GetEnumUnderlyingType();

        else if (type.IsGenericType)
        {
            Type genericType = type.GetGenericTypeDefinition();

            if (IsSupportedCollectionType(genericType))
                type = type.GetGenericArguments()[0];
        }

        bool located = _resolverType2Class.TryGetValue(type, out resolver);
        return located;
    }

    /// <summary>
    /// Tries to fetch the <see cref="IResolver"/> class for the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resolver"></param>
    /// <returns>The located <see cref="IResolver"/> or null</returns>

    public bool TryGetResolver<T>(out IResolver resolver) =>
        TryGetResolver(typeof(T), out resolver);

    /// <summary>
    /// Tries to fetch the ID associated with the type with a resolver.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns>The ushort type or <see cref="ushort.MaxValue"/> if not found.</returns>

    public bool TryGetResolverId<T>(out ushort id)
    {
        foreach (KeyValuePair<ushort, Type> kvp in _resolverId2Type)
        {
            if (kvp.Value == typeof(T))
            {
                id = kvp.Key;
                return true;
            }
        }

        id = ushort.MaxValue;
        return false;
    }

    /// <summary>
    /// Adds or updates an entry in a <see cref="IDataHolder"/>'s custom data dictionary.
    /// </summary>
    /// <param name="dataHolder">The non-null IDataHolder instance</param>
    /// <param name="key">The key cannot be null or empty</param>
    /// <param name="value">Any object including null is permitted</param>
    /// <returns>Whether or not the custom data entry for the <see cref="IDataHolder"/> was added or updated</returns>

    public bool AddOrUpdateCustomDataEntry([NotNull] IDataHolder dataHolder, [NotNull] string key, object value)
    {
        Guard.AgainstNull(dataHolder, "Cannot modify custom data of a null data holder!");

        // TODO: Let user know they used an invalid key.
        if (string.IsNullOrEmpty(key))
            return false;

        Dictionary<string, object> data = GetOrCreateCustomData(dataHolder);
        data[key] = value;
        return true;
    }

    public bool TryGetCustomDataEntry([NotNull] IDataHolder dataHolder, [NotNull] string key, out object value)
    {
        Guard.AgainstNull(dataHolder, "Cannot get custom data of a null data holder!");
        value = null;

        // TODO: Let user know they used an invalid key.
        if (string.IsNullOrEmpty(key))
            return false;

        Dictionary<string, object> data = GetOrCreateCustomData(dataHolder);
        return data.TryGetValue(key, out value);
    }

    public object GetCustomDataEntry([NotNull] IDataHolder dataHolder, [NotNull] string key)
    {
        Guard.AgainstNull(dataHolder, "Cannot get custom data of a null data holder!");

        // TODO: Let user know they used an invalid key.
        if (string.IsNullOrEmpty(key))
            return false;

        Dictionary<string, object> data = GetOrCreateCustomData(dataHolder);
        return data[key];
    }

    /// <summary>
    /// Removes an entry in a <see cref="IDataHolder"/>'s custom data dictionary.
    /// </summary>
    /// <param name="dataHolder">The non-null IDataHolder instance</param>
    /// <param name="key">The key cannot be null or empty</param>
    /// <param name="value">Any object including null is permitted</param>
    /// <returns>Whether or not the custom data entry for the <see cref="IDataHolder"/> was deleted</returns>

    public bool RemoveCustomDataEntry([NotNull] IDataHolder dataHolder, [NotNull] string key, out object value)
    {
        Guard.AgainstNull(dataHolder, "Cannot modify custom data of a null data holder!");
        value = null;

        // TODO: Let user know they used an invalid key.
        if (string.IsNullOrEmpty(key))
            return false;

        Dictionary<string, object> data = GetOrCreateCustomData(dataHolder);
        return data.Remove(key, out value);
    }

    /// <summary>
    /// Fetches the entire map of custom data for the specified <see cref="IDataHolder"/>
    /// </summary>
    /// <param name="dataHolder"></param>
    /// <returns>The entire dictionary of custom data for the specified <see cref="IDataHolder"/></returns>
    public Dictionary<string, object> GetOrCreateCustomData([NotNull] IDataHolder dataHolder)
    {
        Guard.AgainstNull(dataHolder, "Cannot locate custom data for a null data holder!");
        return _customDataMap.GetValue(dataHolder, _ => new(StringComparer.Ordinal));
    }

    /// <summary>
    /// Deletes the custom data associated with the specified <see cref="IDataHolder"/> (if it exists)
    /// </summary>
    /// <param name="dataHolder"></param>
    /// <returns>Whether or not a dictionary of custom data was deleted</returns>

    public bool DeleteCustomData([NotNull] IDataHolder dataHolder)
    {
        Guard.AgainstNull(dataHolder, "Cannot delete custom data for a null data holder!");
        return _customDataMap.Remove(dataHolder);
    }

    /// <summary>
    /// Checks if a dictionary of custom data exists for the specified <see cref="IDataHolder"/>
    /// </summary>
    /// <param name="dataHolder"></param>
    /// <returns>Whether or not data exists for the specified <see cref="IDataHolder"/></returns>

    public bool HasCustomData([NotNull] IDataHolder dataHolder) =>
        dataHolder is not null && _customDataMap.TryGetValue(dataHolder, out _);

    public void Dispose()
    {
        PayloadPool.Clear();
        _customDataMap.Clear();
    }

}
