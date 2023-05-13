using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
  public static class Mapper<TI, TO> where TO : new()
  {
    private static readonly Func<TI, TO, TO> _map;
    private static readonly Func<TI, TO, TO> _merge;
    private static readonly IEnumerable<PropertyInfo> _source;
    private static readonly IEnumerable<PropertyInfo> _destination;

    static Mapper()
    {
      _destination = typeof(TO)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(o => o.CanWrite);

      _source = typeof(TI)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(o => o.CanRead);

      _map = CreateMap();
      _merge = CreateMerge();
    }

    private static Func<TI, TO, TO> CreateMap()
    {
      var input = Expression.Parameter(typeof(TI), nameof(TI));
      var output = Expression.Parameter(typeof(TO), nameof(TO));
      var assignments = _source.Join(_destination,
        source => source.Name,
        destination => destination.Name,
        (source, destination) =>
          Expression.Bind(destination,
            Expression.Property(input, source)));

      var content = Expression.MemberInit(Expression.New(typeof(TO)), assignments);
      var expression = Expression.Lambda<Func<TI, TO, TO>>(content, input, output);

      return expression.Compile();
    }

    private static Func<TI, TO, TO> CreateMerge()
    {
      var input = Expression.Parameter(typeof(TI), nameof(TI));
      var output = Expression.Parameter(typeof(TO), nameof(TO));
      var sourceMembers = new Dictionary<string, MemberExpression>();

      foreach (var member in _source)
      {
        sourceMembers[member.Name] = Expression.Property(input, member);
      }

      var assignments = _destination.Select(o =>
      {
        if (sourceMembers.TryGetValue(o.Name, out var sourceProperty))
        {
          var isValue = sourceProperty.Type.IsValueType;
          var isNullable = Nullable.GetUnderlyingType(sourceProperty.Type);

          return isNullable is not null || isValue is false ?
            Expression.Bind(o, Expression.Coalesce(sourceProperty, Expression.Property(output, o))) :
            Expression.Bind(o, sourceProperty);
        }

        return Expression.Bind(o, Expression.Property(output, o));
      });

      var content = Expression.MemberInit(Expression.New(typeof(TO)), assignments);
      var expression = Expression.Lambda<Func<TI, TO, TO>>(content, input, output);

      return expression.Compile();
    }

    public static TO Copy(TI input) => _map(input, new TO());

    public static TO Map(TI input, TO output) => _map(input, output);

    public static TO Merge(TI input, TO output) => _merge(input, output);
  }
}
