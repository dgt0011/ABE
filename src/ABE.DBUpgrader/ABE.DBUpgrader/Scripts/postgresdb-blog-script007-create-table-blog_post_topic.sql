CREATE TABLE IF NOT EXISTS blog_post_topic (
	id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	post_id BIGINT NOT NULL,
	topic_id BIGINT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS pk_idx_blog_post_topic_post_id ON blog_post_topic (post_id,topic_id);

GRANT SELECT, INSERT, UPDATE ON TABLE blog_post_topic TO blog;