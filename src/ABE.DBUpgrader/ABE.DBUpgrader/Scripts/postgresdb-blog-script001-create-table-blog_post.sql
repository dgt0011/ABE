CREATE TABLE IF NOT EXISTS blog_post (
	id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	title VARCHAR(200) NOT NULL,
	description VARCHAR(450) NOT NULL,
	slug VARCHAR(450) NOT NULL,
	status INT NOT NULL DEFAULT(0),
	view_count INT NOT NULL DEFAULT(0),
	published_datetime TIMESTAMPTZ NULL,
	created_datetime TIMESTAMPTZ NULL,
	cover_img_path VARCHAR(200) NOT NULL,
	body TEXT NULL
);

CREATE INDEX IF NOT EXISTS idx1_blog_post_id_status ON blog_post (id, status);

GRANT SELECT, INSERT, UPDATE ON TABLE blog_post TO blog;