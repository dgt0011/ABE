CREATE TABLE IF NOT EXISTS blog_post_tag (
	id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	post_id BIGINT NOT NULL,
	tag_id BIGINT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS pk_idx_blog_post_tag_post_id ON blog_post_tag (post_id, tag_id);

GRANT SELECT, INSERT, UPDATE ON TABLE blog_post_tag TO blog;