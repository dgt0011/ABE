using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Providers;
using AnotherBlogEngine.Web.Shared.Models;
using AutoMapper;

namespace AnotherBlogEngine.Core
{
    public sealed class Mappings
    {
        private static readonly Lazy<Mappings> Lazy = new(() => new Mappings());

        public static Mappings Instance => Lazy.Value;

        public IMapper Mapper { get; private set; } = null!;

        private Mappings()
        {
            AutoMapConfiguration();
        }

        private void AutoMapConfiguration()
        {
            var config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<TopicDto, TopicDataItem>()
                        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
                        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                        .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.date_created))
                        .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.date_updated))
                        .ReverseMap()
                        .ForAllMembers(opts=>opts.Condition((src, dest, srcMember) => srcMember is not null));

                    cfg.CreateMap<TermDto, TermDataItem>()
                        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                        .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.key))
                        .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.text))
                        .ReverseMap()
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));

                    cfg.CreateMap<PostSummaryDto, PostSummaryItem>()
                        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
                        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                        .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                        .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.view_count))
                        .ForMember(dest => dest.CreatedDateTime, opt => opt.MapFrom(src => src.created_datetime))
                        .ForMember(dest => dest.PublishedDateTime, opt => opt.MapFrom(src => src.published_datetime))
                        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                        .ForMember(dest => dest.CoverImagePath, opt => opt.MapFrom(src => src.cover_img_path))
                        .ReverseMap()
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));


                    cfg.CreateMap<PostDetailsDto, PostItem>()
                        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
                        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                        .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                        .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.view_count))
                        .ForMember(dest => dest.CreatedDateTime, opt => opt.MapFrom(src => src.created_datetime))
                        .ForMember(dest => dest.PublishedDateTime, opt => opt.MapFrom(src => src.published_datetime))
                        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                        .ForMember(dest => dest.CoverImagePath, opt => opt.MapFrom(src => src.cover_img_path))
                        .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.body))

                        .ReverseMap()
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));


                    cfg.AllowNullCollections = true;
                }
            );
            Mapper = config.CreateMapper();
        }
    }
}
