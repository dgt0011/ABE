CREATE TABLE IF NOT EXISTS term (
	id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	key VARCHAR(200) NOT NULL,
	text VARCHAR(450) NOT NULL,
	deleted_fg BOOL NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX IF NOT EXISTS pk_idx_term_key ON term (key);

GRANT SELECT, INSERT, UPDATE ON TABLE term TO blog;

INSERT INTO term (key, text)
SELECT 'BlogTitle', 'Bits & Bytes'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'BlogTitle');

INSERT INTO term (key, text)
SELECT 'BlogDescription', 'A random collection of notes, personal tips, and general things.'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'BlogDescription');

INSERT INTO term (key, text)
SELECT 'TopicsLabel', 'Topics'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'TopicsLabel');

INSERT INTO term (key, text)
SELECT 'SearchLabel', 'Search'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'SearchLabel');

INSERT INTO term (key, text)
SELECT 'LoginLabel', 'Login'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'LoginLabel');

INSERT INTO term (key, text)
SELECT 'NoPostsMessage', 'Currently no posts are available.'
WHERE NOT EXISTS (SELECT key FROM term WHERE key = 'NoPostsMessage');

INSERT INTO term (key, text)
SELECT 'PublishedLabel', 'published'
WHERE NOT EXISTS (select key FROM term WHERE key = 'PublishedLabel');