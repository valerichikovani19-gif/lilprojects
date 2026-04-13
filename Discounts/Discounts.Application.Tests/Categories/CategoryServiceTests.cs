using Discounts.Application.DTOs.Category;
using Discounts.Application.Exceptions;
using Discounts.Application.Tests.Categories.data;
using Discounts.Domain.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Discounts.Application.Tests.Categories
{
    public class CategoryServiceTests : IClassFixture<CategoryServiceFixture>
    {
        private readonly CategoryServiceFixture _fixture;

        public CategoryServiceTests(CategoryServiceFixture fixture)
        {
            _fixture = fixture;
            //reseting mocks
            _fixture.CategoryRepoMock.Reset();
            _fixture.MapperMock.Reset();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Create -  Should call AddAsync and return Id")]
        public async Task Create_WhenValid_ShouldReturnId()
        {
            //arrange
            var dto = new CreateCategoryDto { Name = "Food" };
            var entity = new Category { Id = 5, Name = "Food" };

            //mapper setup
            _fixture.MapperMock.Setup(m => m.Map<Category>(dto)).Returns(entity);

            // act
            var result = await _fixture.Service.CreateCategoryAsync(dto, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                result.Should().Be(5);
                _fixture.CategoryRepoMock.Verify(x => x.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
                _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact(DisplayName = "Update - Should throw CategoryNotFoundException when Id invalid")]
        public async Task Update_WhenIdNotExists_ShouldThrowException()
        {
            //Arrange
            var id = 99;
            _fixture.CategoryRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            //act
            var action = async () => await _fixture.Service.UpdateCategoryAsync(id, new UpdateCategoryDto(), CancellationToken.None);

            //assert
            await action.Should().ThrowAsync<CategoryNotFoundException>();
        }

        [Fact(DisplayName = "Update -  Should modify Name and call update")]
        public async Task Update_WhenValid_ShouldUpdateEntity()
        {
            //arrange
            var id = 1;
            var category = new Category { Id = id, Name = "Old Name" };
            var updateDto = new UpdateCategoryDto { Name = "New Name" };

            _fixture.CategoryRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            //act
            await _fixture.Service.UpdateCategoryAsync(id, updateDto, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                category.Name.Should().Be("New Namee"); //propert should change in memory
                _fixture.CategoryRepoMock.Verify(x => x.Update(category), Times.Once);
                _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact(DisplayName = "Delete - Should throw CategoryNotFoundException when Id invalid")]
        public async Task Delete_WhenIdNotExists_ShouldThrowException()
        {
            // arrange
            var id = 99;
            _fixture.CategoryRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            //act
            var action = async () => await _fixture.Service.DeleteCategoryAsync(id, CancellationToken.None);

            //Assert
            await action.Should().ThrowAsync<CategoryNotFoundException>();
        }

        [Fact(DisplayName = "Delete - Should call delete on Repository")]
        public async Task Delete_WhenValid_ShouldCallDelete()
        {
            //arrange
            var id = 1;
            var category = new Category { Id = id, Name = "To Delete" };

            _fixture.CategoryRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            //act
            await _fixture.Service.DeleteCategoryAsync(id, CancellationToken.None);

            //assert
            _fixture.CategoryRepoMock.Verify(x => x.Delete(category), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
