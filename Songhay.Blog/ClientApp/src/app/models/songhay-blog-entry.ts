/**Defines the conventional Blog entry. */
export class BlogEntry {
    /**
     * Gets or sets the author.
     * @value
     * The author.
     */
    Author: string;

    /**
     * Gets or sets the content.
     * @value
     * The content.
     */
    Content: string;

    /**
     * Gets or sets the incept date.
     * @value
     * The incept date.
     */
    InceptDate: Date;

    /**
     * Gets or sets the published state.
     * @value
     * The published state.
     */
    IsPublished: boolean | null;

    /**
     * Gets or sets the item category.
     * @value
     * The item category.
     */
    ItemCategory: string;

    /**
     * Gets or sets the item category.
     * @value
     * The item category.
     */
    ItemCategoryObject: object;

    /**
     * Gets or sets the modification date.
     * @value
     * The modification date.
     */
    ModificationDate: Date;

    /**
     * Gets or sets the slug.
     * @value
     * The slug.
     */
    Slug: string;

    /**
     * Gets or sets the sort ordinal.
     * @value
     * The sort ordinal.
     */
    SortOrdinal: string;

    /**
     * Gets or sets the tag.
     * @value
     * The tag.
     */
    Tag: string;

    /**
     * Gets or sets the title.
     * @value
     * The title.
     */
    Title: string;
}
