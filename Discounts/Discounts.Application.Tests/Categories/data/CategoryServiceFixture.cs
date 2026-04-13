using AutoMapper;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.Services;
using Moq;
namespace Discounts.Application.Tests.Categories.data
{
    public class CategoryServiceFixture : IDisposable
    {
        public CategoryService Service { get; }

        // Mocks
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<ICategoryRepository> CategoryRepoMock { get; }
        public Mock<IMapper> MapperMock { get; }

        public CategoryServiceFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            CategoryRepoMock = new Mock<ICategoryRepository>();
            MapperMock = new Mock<IMapper>();

            UnitOfWorkMock.Setup(u => u.Categories).Returns(CategoryRepoMock.Object);

            
            Service = new CategoryService(UnitOfWorkMock.Object, MapperMock.Object);
        }
        public void Dispose()
        {
        }
    }
}
