using System;

namespace OpenClosedPrinciple
{
    public enum Color
    {
        Red, Green, Blue
    }
    public enum Size
    {
        Small, Medium, Large
    }
    public class Product
    {
        public readonly string Name;
        public readonly Color Color;
        public readonly Size Size;

        public Product(string name, Color color, Size size)
        {
            Name = name;
            Color = color;
            Size = size;
        }
    }
    public class ProductFilret
    {
        public IEnumerable<Product> FilterByColor(
            IEnumerable<Product> products, Color color)
        {
            foreach (var item in products)
                if (item.Color == color)
                    yield return item;
        }
        public IEnumerable<Product> FilterBySize(IEnumerable<Product> products, Size size)
        {
            foreach (var item in products)
                if (item.Size == size)
                    yield return item;
        }
        public IEnumerable<Product> FilterBySizeAndColor
            (IEnumerable<Product> products, Size size, Color color)
        {
            foreach (var item in products)
                if (item.Size == size && item.Color == color)
                    yield return item;
        }

        // не маштабируется получается ----> state space explosion
        // надо все время добавлять много методов для расширения 
    }

    #region паттерн спецификация  
    public abstract class Specification<T> // есть какая то спецификация 
    {
        // которая проверяет удовлетворяет item конкретному критерию или нет 
        public abstract bool IsSatisfied(T item); 

        public static Specification<T> operator &(Specification<T> first, 
            Specification<T> second)
        {
            return new AndSpecification<T>(first, second);
        }
    }

    public interface IFilter<T>
    {
        IEnumerable<T> Filter(IEnumerable<T> items, Specification<T> spec);
    }

    public class BetterFilter : IFilter<Product>
    {
        public IEnumerable<Product> Filter(IEnumerable<Product> items,
            Specification<Product> spec)
        {
            foreach (var item in items)
               if(spec.IsSatisfied(item))
                     yield return item;
        }
    }

    public class ColorSpecification : Specification<Product>
    {
        private readonly Color color;
        public ColorSpecification(Color color)
        {
            this.color = color;
        }
        public override bool IsSatisfied(Product item)
        {
            return item.Color == color; 
        }
    }

    public class SizeSpecification : Specification<Product>
    {
        private readonly Size size;
        public SizeSpecification(Size size)
        {
            this.size = size;
        }
        public override bool IsSatisfied(Product item)
        {
            return item.Size == size;
        }
    }

    // combinator
    public class AndSpecification<Product> : Specification<Product>
    {
        private readonly Specification<Product> first, second;
        public AndSpecification(Specification<Product> first, 
            Specification<Product> second)
        {
            this.first = first;
            this.second = second;
        }

        public override bool IsSatisfied(Product item)
        {
           return first.IsSatisfied(item) && second.IsSatisfied(item);
        }
    }

    #endregion
    internal class Program
    {
        static void Main(string[] args)
        {
            var apple = new Product("Aple", Color.Green, Size.Small);
            var tree = new Product("Tree", Color.Green, Size.Medium);
            var house = new Product("House", Color.Blue, Size.Large);

            Product[] products = { apple, tree, house };

            var pf = new ProductFilret();
            Console.WriteLine("Green products (old):");
            foreach (var item in pf.FilterByColor(products, Color.Green))
                Console.WriteLine($"- {item.Name} is green");

            Console.WriteLine();
            var bf = new BetterFilter();
            Console.WriteLine("Blue products (Паттерн спецификация):");
            var colorSpec = new ColorSpecification(Color.Blue);
            foreach (var item in bf.Filter(products,colorSpec))
            Console.WriteLine($"- {item.Name} is blue");

            Console.WriteLine();
            Console.WriteLine("Blue and large products (Паттерн combinator):");
            var largeBlueSpec = 
                new AndSpecification<Product>(new SizeSpecification(Size.Large), colorSpec);
            foreach (var item in bf.Filter(products, largeBlueSpec))
            Console.WriteLine($"- {item.Name} is blue and large");
        }
    }
}