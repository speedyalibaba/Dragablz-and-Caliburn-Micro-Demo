using System;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Links.Common.Controls
{
	/// <summary>
	/// Provides methods that allow getting property values without reflection.
	/// </summary>
	public class BindingExpressionHelper : FrameworkElement
	{
		#region Fields

		private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(BindingExpressionHelper), null);

		#endregion Fields

		#region Methods

		/// <summary>
		/// Returns a Function that will return the value of the property, specified by the provided propertyPath.
		/// </summary>
		/// <param name="itemType">The type of the instance which property will be returned.</param>
		/// <param name="propertyPath">The path of the property which value will be returned.</param>
		public static Func<object, object> CreateGetValueFunc(Type itemType, string propertyPath)
		{
			if (!itemType.IsPublic && !itemType.IsNestedPublic)
			{
				goto IL_004e;
			}
			if (propertyPath != null && propertyPath.IndexOfAny(new char[6]
			{
				'.',
				'[',
				']',
				'(',
				')',
				'@'
			}) > -1)
			{
				goto IL_004e;
			}
			if (!typeof(DataRow).IsAssignableFrom(itemType) && !typeof(ICustomTypeDescriptor).IsAssignableFrom(itemType))
			{
				ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(itemType, "item");
				System.Linq.Expressions.Expression body;
				if (string.IsNullOrEmpty(propertyPath))
				{
					body = parameterExpression;
				}
				else
				{
					try
					{
						body = System.Linq.Expressions.Expression.PropertyOrField(parameterExpression, propertyPath);
					}
					catch (ArgumentException)
					{
						return (object p) => null;
					}
				}
				LambdaExpression lambdaExpression = System.Linq.Expressions.Expression.Lambda(body, parameterExpression);
				Delegate @delegate = lambdaExpression.Compile();
				MethodInfo methodInfo = typeof(BindingExpressionHelper).GetMethod("ToUntypedFunc", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(itemType, lambdaExpression.Body.Type);
				return (Func<object, object>)methodInfo.Invoke(null, new object[1]
				{
					@delegate
				});
			}
			return (object item) => GetValueThroughBinding(item, propertyPath);
			IL_004e:
			return (object item) => GetValueThroughBinding(item, propertyPath);
		}

		/// <summary>
		/// Gets the value of the property specified by the provided propertyPath.
		/// </summary>
		/// <param name="item">The instance which property value will be returned.</param>
		/// <param name="propertyPath">The path of the property which value will be returned.</param>
		public static object GetValue(object item, string propertyPath)
		{
			if (item == null)
			{
				return null;
			}
			return CreateGetValueFunc(item.GetType(), propertyPath)(item);
		}

		/// <summary>
		/// Gets the value of the specified item using the provided Binding.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="binding">The binding.</param>
		/// <returns>The value of the item.</returns>
		public static object GetValue(object item, Binding binding)
		{
			return GetValueThroughBinding(item, binding);
		}

		private static object GetValueThroughBinding(object item, Binding binding)
		{
			BindingExpressionHelper bindingExpressionHelper = new BindingExpressionHelper();
			try
			{
				bindingExpressionHelper.DataContext = item;
				BindingOperations.SetBinding(bindingExpressionHelper, ValueProperty, binding);
				return bindingExpressionHelper.GetValue(ValueProperty);
			}
			finally
			{
				bindingExpressionHelper.ClearValue(ValueProperty);
			}
		}

		private static object GetValueThroughBinding(object item, string propertyPath)
		{
			return GetValueThroughBinding(item, new Binding(propertyPath ?? "."));
		}

		private static Func<object, object> ToUntypedFunc<T, TResult>(Func<T, TResult> func)
		{
			return (object item) => func((T)item);
		}

		#endregion Methods
	}
}