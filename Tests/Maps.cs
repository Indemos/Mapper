using Mapper;

namespace Tests
{
  public class Maps
  {
    public class A
    {
      public int Id { get; set; }
      public int? Count { get; set; }
      public string Name { get; set; }
    }

    public class B
    {
      public int Id { get; set; }
      public int? Count { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public A Link { get; set; }

    }

    [Fact]
    public void MergeSameTypes()
    {
      var source = new A { Id = 1 };
      var destination = new A { Id = 2, Name = "2" };

      var merge = Mapper<A, A>.Merge(source, destination);

      Assert.Equal(1, merge.Id);
      Assert.Equal("2", merge.Name);
    }

    [Fact]
    public void Merge()
    {
      var source = new A { Id = 1 };
      var destination = new B { Id = 2, Count = 5, Name = "2", Link = new A { Id = 3 }};

      var merge = Mapper<A, B>.Merge(source, destination);

      Assert.Equal(1, merge.Id);
      Assert.Equal(5, merge.Count);
      Assert.Equal("2", merge.Name);
      Assert.Equal(3, merge.Link.Id);
      Assert.Equal(destination.Link, merge.Link);
    }

    [Fact]
    public void Map()
    {
      var source = new B { Id = 1 };
      var destination = new B { Id = 2, Name = "2", Link = new A { Id = 3 } };

      var merge = Mapper<B, B>.Map(source, destination);

      Assert.Equal(1, merge.Id);
      Assert.Null(merge.Name);
      Assert.Null(merge.Link);
    }
  }
}