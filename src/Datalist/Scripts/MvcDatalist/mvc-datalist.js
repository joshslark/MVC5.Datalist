/*!
 * Datalist 7.2.1
 * https://github.com/NonFactors/MVC5.Datalist
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
var MvcDatalistFilter = (function () {
    function MvcDatalistFilter(datalist) {
        var filter = this;
        var data = datalist.group.dataset;

        filter.offset = 0;
        filter.datalist = datalist;
        filter.sort = data.sort || '';
        filter.order = data.order || '';
        filter.search = data.search || '';
        filter.rows = parseInt(data.rows) || 20;
        filter.additional = (data.filters || '').split(',').filter(Boolean);
    }

    MvcDatalistFilter.prototype = {
        formUrl: function (search) {
            var encode = encodeURIComponent;
            var url = this.datalist.url.split('?')[0];
            var urlQuery = this.datalist.url.split('?')[1];
            var filter = this.datalist.extend({ ids: [], checkIds: [], selected: [] }, this, search);
            var query = '?' + (urlQuery ? urlQuery + '&' : '') + 'search=' + encode(filter.search);

            filter.additional.forEach(function (name) {
                [].forEach.call(document.querySelectorAll('[name="' + name + '"]'), function (filter) {
                    query += '&' + encode(name) + '=' + encode(filter.value);
                });
            });

            filter.selected.forEach(function (selected) {
                query += '&selected=' + encode(selected.Id);
            });

            filter.checkIds.forEach(function (id) {
                query += '&checkIds=' + encode(id.value);
            });

            filter.ids.forEach(function (id) {
                query += '&ids=' + encode(id.value);
            });

            query += '&sort=' + encode(filter.sort) +
                '&order=' + encode(filter.order) +
                '&offset=' + encode(filter.offset) +
                '&rows=' + encode(filter.rows) +
                '&_=' + Date.now();

            return url + query;
        }
    };

    return MvcDatalistFilter;
}());
var MvcDatalistDialog = (function () {
    function MvcDatalistDialog(datalist) {
        var dialog = this;
        var element = document.getElementById(datalist.group.dataset.dialog || 'DatalistDialog');

        dialog.element = element;
        dialog.datalist = datalist;
        dialog.title = datalist.group.dataset.title || '';
        dialog.options = { preserveSearch: true, rows: { min: 1, max: 99 }, openDelay: 100 };

        dialog.overlay = new MvcDatalistOverlay(this);
        dialog.table = element.querySelector('table');
        dialog.tableHead = element.querySelector('thead');
        dialog.tableBody = element.querySelector('tbody');
        dialog.rows = element.querySelector('.datalist-rows');
        dialog.header = element.querySelector('.datalist-title');
        dialog.search = element.querySelector('.datalist-search');
        dialog.footer = element.querySelector('.datalist-load-more');
        dialog.selector = element.querySelector('.datalist-selector');
        dialog.closeButton = element.querySelector('.datalist-close');
        dialog.error = element.querySelector('.datalist-dialog-error');
        dialog.loader = element.querySelector('.datalist-dialog-loader');
    }

    MvcDatalistDialog.prototype = {
        open: function () {
            var dialog = this;
            var filter = dialog.datalist.filter;
            MvcDatalistDialog.prototype.current = dialog;

            filter.offset = 0;
            filter.search = dialog.options.preserveSearch ? filter.search : '';

            dialog.error.style.display = 'none';
            dialog.loader.style.display = 'none';
            dialog.header.innerText = dialog.title;
            dialog.rows.value = dialog.limitRows(filter.rows);
            dialog.selected = dialog.datalist.selected.slice();
            dialog.error.innerHTML = dialog.datalist.lang.error;
            dialog.footer.innerText = dialog.datalist.lang.more;
            dialog.search.placeholder = dialog.datalist.lang.search;
            dialog.selector.style.display = dialog.datalist.multi ? '' : 'none';
            dialog.selector.innerText = dialog.datalist.lang.select.replace('{0}', dialog.datalist.selected.length);

            dialog.bind();
            dialog.refresh();
            dialog.search.value = filter.search;

            setTimeout(function () {
                if (dialog.isLoading) {
                    dialog.loader.style.opacity = 1;
                    dialog.loader.style.display = '';
                }

                dialog.overlay.show();
                dialog.search.focus();
            }, dialog.options.openDelay);
        },
        close: function () {
            var dialog = MvcDatalistDialog.prototype.current;
            dialog.datalist.group.classList.remove('datalist-error');

            dialog.datalist.select(dialog.selected, true);
            dialog.datalist.stopLoading();
            dialog.overlay.hide();

            if (dialog.datalist.browser) {
                dialog.datalist.browser.focus();
            }

            MvcDatalistDialog.prototype.current = null;
        },
        refresh: function () {
            var dialog = this;
            dialog.isLoading = true;
            dialog.error.style.opacity = 0;
            dialog.error.style.display = '';
            dialog.loader.style.display = '';
            var loading = setTimeout(function () {
                dialog.loader.style.opacity = 1;
            }, dialog.datalist.options.loadingDelay);

            dialog.datalist.startLoading({ selected: dialog.selected, rows: dialog.datalist.filter.rows + 1 }, function (data) {
                dialog.isLoading = false;
                clearTimeout(loading);
                dialog.render(data);
            }, function () {
                dialog.isLoading = false;
                clearTimeout(loading);
                dialog.render();
            });
        },

        render: function (data) {
            var dialog = this;

            if (!dialog.datalist.filter.offset) {
                dialog.tableBody.innerHTML = '';
                dialog.tableHead.innerHTML = '';
            }

            dialog.loader.style.opacity = 0;

            setTimeout(function () {
                dialog.loader.style.display = 'none';
            }, dialog.datalist.options.loadingDelay);

            if (data) {
                dialog.error.style.display = 'none';

                if (!dialog.datalist.filter.offset) {
                    dialog.renderHeader(data.Columns);
                }

                dialog.renderBody(data);

                if (data.Rows.length <= dialog.datalist.filter.rows) {
                    dialog.footer.style.display = 'none';
                } else {
                    dialog.footer.style.display = '';
                }
            } else {
                dialog.error.style.opacity = 1;
            }
        },
        renderHeader: function (columns) {
            var row = document.createElement('tr');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].Hidden) {
                    row.appendChild(this.createHeaderCell(columns[i]));
                }
            }

            row.appendChild(document.createElement('th'));
            this.tableHead.appendChild(row);
        },
        renderBody: function (data) {
            var dialog = this;

            data.Selected.forEach(function (selected) {
                var row = dialog.createDataRow(data.Columns, selected);
                row.className = 'selected';

                dialog.tableBody.appendChild(row);
            });

            if (data.Selected.length) {
                var separator = document.createElement('tr');
                var content = document.createElement('td');

                content.colSpan = data.Columns.length + 1;
                separator.className = 'datalist-split';

                dialog.tableBody.appendChild(separator);
                separator.appendChild(content);
            }

            for (var i = 0; i < data.Rows.length && i < dialog.datalist.filter.rows; i++) {
                dialog.tableBody.appendChild(dialog.createDataRow(data.Columns, data.Rows[i]));
            }

            if (!data.Rows.length && !dialog.datalist.filter.offset) {
                var container = document.createElement('tr');
                var empty = document.createElement('td');

                empty.innerHTML = dialog.datalist.lang.noData;
                container.className = 'datalist-empty';
                empty.colSpan = data.Columns.length + 1;

                dialog.tableBody.appendChild(container);
                container.appendChild(empty);
            }
        },

        createHeaderCell: function (column) {
            var dialog = this;
            var filter = dialog.datalist.filter;
            var header = document.createElement('th');

            if (column.CssClass) {
                header.classList.add(column.CssClass);
            }

            if (filter.sort == column.Key) {
                header.classList.add('datalist-' + filter.order.toLowerCase());
            }

            header.innerText = column.Header || '';
            header.addEventListener('click', function () {
                filter.order = filter.sort == column.Key && filter.order == 'Asc' ? 'Desc' : 'Asc';
                filter.sort = column.Key;
                filter.offset = 0;

                dialog.refresh();
            });

            return header;
        },
        createDataRow: function (columns, data) {
            var dialog = this;
            var datalist = dialog.datalist;
            var row = document.createElement('tr');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].Hidden) {
                    var cell = document.createElement('td');
                    cell.className = columns[i].CssClass || '';
                    cell.innerText = data[columns[i].Key] || '';

                    row.appendChild(cell);
                }
            }

            row.appendChild(document.createElement('td'));

            row.addEventListener('click', function (e) {
                if (!window.getSelection().isCollapsed) {
                    return;
                }

                if (datalist.multi) {
                    var index = datalist.indexOf(dialog.selected, data.Id);

                    if (index >= 0) {
                        dialog.selected.splice(index, 1);

                        this.classList.remove('selected');
                    } else {
                        dialog.selected.push(data);

                        this.classList.add('selected');
                    }

                    dialog.selector.innerText = datalist.lang.select.replace('{0}', dialog.selected.length);
                } else {
                    if (e.ctrlKey && datalist.indexOf(dialog.selected, data.Id) >= 0) {
                        dialog.selected = [];
                    } else {
                        dialog.selected = [data];
                    }

                    dialog.close();
                }
            });

            return row;
        },

        limitRows: function (value) {
            value = Math.max(this.options.rows.min, Math.min(parseInt(value), this.options.rows.max));

            return isNaN(value) ? this.datalist.filter.rows : value;
        },

        bind: function () {
            var dialog = this;

            dialog.selector.addEventListener('click', dialog.close);
            dialog.footer.addEventListener('click', dialog.loadMore);
            dialog.rows.addEventListener('change', dialog.rowsChanged);
            dialog.closeButton.addEventListener('click', dialog.close);
            dialog.search.addEventListener('keyup', dialog.searchChanged);
        },
        loadMore: function () {
            var dialog = MvcDatalistDialog.prototype.current;

            dialog.datalist.filter.offset += dialog.datalist.filter.rows;

            dialog.refresh();
        },
        rowsChanged: function () {
            var rows = this;
            var dialog = MvcDatalistDialog.prototype.current;

            rows.value = dialog.limitRows(rows.value);

            if (dialog.datalist.filter.rows != rows.value) {
                dialog.datalist.filter.rows = parseInt(rows.value);
                dialog.datalist.filter.offset = 0;

                dialog.refresh();
            }
        },
        searchChanged: function (e) {
            var search = this;
            var dialog = MvcDatalistDialog.prototype.current;

            dialog.datalist.stopLoading();
            clearTimeout(dialog.searching);
            dialog.searching = setTimeout(function () {
                if (dialog.datalist.filter.search != search.value || e.keyCode == 13) {
                    dialog.datalist.filter.search = search.value;
                    dialog.datalist.filter.offset = 0;

                    dialog.refresh();
                }
            }, dialog.datalist.options.searchDelay);
        }
    };

    return MvcDatalistDialog;
}());
var MvcDatalistOverlay = (function () {
    function MvcDatalistOverlay(dialog) {
        this.element = this.findOverlay(dialog.element);
        this.dialog = dialog;

        this.bind();
    }

    MvcDatalistOverlay.prototype = {
        findOverlay: function (element) {
            var overlay = element;

            if (!overlay) {
                throw new Error('Datalist dialog element was not found.');
            }

            while (overlay && !overlay.classList.contains('datalist-overlay')) {
                overlay = overlay.parentElement;
            }

            if (!overlay) {
                throw new Error('Datalist dialog has to be inside a datalist-overlay.');
            }

            return overlay;
        },

        show: function () {
            var body = document.body.getBoundingClientRect();
            if (body.left + body.right < window.innerWidth) {
                var scrollWidth = window.innerWidth - document.body.clientWidth;
                var paddingRight = parseFloat(getComputedStyle(document.body).paddingRight);

                document.body.style.paddingRight = paddingRight + scrollWidth + 'px';
            }

            document.body.classList.add('datalist-open');
            this.element.style.display = 'block';
        },
        hide: function () {
            document.body.classList.remove('datalist-open');
            document.body.style.paddingRight = '';
            this.element.style.display = '';
        },

        bind: function () {
            this.element.addEventListener('mousedown', this.onMouseDown);
            document.addEventListener('keydown', this.onKeyDown);
        },
        onMouseDown: function (e) {
            var targetClasses = e.target.classList;

            if (targetClasses.contains('datalist-overlay') || targetClasses.contains('datalist-wrapper')) {
                MvcDatalistDialog.prototype.current.close();
            }
        },
        onKeyDown: function (e) {
            if (e.which == 27 && MvcDatalistDialog.prototype.current) {
                MvcDatalistDialog.prototype.current.close();
            }
        }
    };

    return MvcDatalistOverlay;
}());
var MvcDatalistAutocomplete = (function () {
    function MvcDatalistAutocomplete(datalist) {
        var autocomplete = this;

        autocomplete.datalist = datalist;
        autocomplete.element = document.createElement('ul');
        autocomplete.element.className = 'datalist-autocomplete';
        autocomplete.options = { minLength: 1, rows: 20, sort: datalist.filter.sort, order: datalist.filter.order };
    }

    MvcDatalistAutocomplete.prototype = {
        search: function (term) {
            var autocomplete = this;
            var datalist = autocomplete.datalist;

            datalist.stopLoading();
            clearTimeout(autocomplete.searching);
            autocomplete.searching = setTimeout(function () {
                if (term.length < autocomplete.options.minLength || datalist.readonly) {
                    autocomplete.hide();

                    return;
                }

                datalist.startLoading({
                    search: term,
                    selected: datalist.multi ? datalist.selected : [],
                    sort: autocomplete.options.sort,
                    order: autocomplete.options.order,
                    offset: 0,
                    rows: autocomplete.options.rows
                }, function (data) {
                    autocomplete.hide();

                    for (var i = 0; i < data.Rows.length; i++) {
                        var item = document.createElement('li');
                        item.innerText = data.Rows[i].Label;
                        item.dataset.id = data.Rows[i].Id;

                        autocomplete.element.appendChild(item);
                        autocomplete.bind(item, [data.Rows[i]]);

                        if (i == 0) {
                            autocomplete.activeItem = item;
                            item.classList.add('active');
                        }
                    }

                    if (!data.Rows.length) {
                        var noData = document.createElement('li');
                        noData.className = 'datalist-autocomplete-no-data';
                        noData.innerText = datalist.lang.noData;

                        autocomplete.element.appendChild(noData);
                    }

                    autocomplete.show();
                });
            }, autocomplete.datalist.options.searchDelay);
        },
        previous: function () {
            var autocomplete = this;

            if (!autocomplete.element.parentElement || !autocomplete.activeItem) {
                autocomplete.search(autocomplete.datalist.search.value);

                return;
            }

            autocomplete.activeItem.classList.remove('active');
            autocomplete.activeItem = autocomplete.activeItem.previousElementSibling || autocomplete.element.lastElementChild;
            autocomplete.activeItem.classList.add('active');
        },
        next: function () {
            var autocomplete = this;

            if (!autocomplete.element.parentElement || !autocomplete.activeItem) {
                autocomplete.search(autocomplete.datalist.search.value);

                return;
            }

            autocomplete.activeItem.classList.remove('active');
            autocomplete.activeItem = autocomplete.activeItem.nextElementSibling || autocomplete.element.firstElementChild;
            autocomplete.activeItem.classList.add('active');
        },
        show: function () {
            var autocomplete = this;
            var search = autocomplete.datalist.search.getBoundingClientRect();

            autocomplete.element.style.left = search.left + window.pageXOffset + 'px';
            autocomplete.element.style.top = search.top + search.height + window.pageYOffset + 'px';

            document.body.appendChild(autocomplete.element);
        },
        hide: function () {
            var autocomplete = this;

            autocomplete.activeItem = null;
            autocomplete.element.innerHTML = '';

            if (autocomplete.element.parentElement) {
                document.body.removeChild(autocomplete.element);
            }
        },

        bind: function (item, data) {
            var autocomplete = this;
            var datalist = autocomplete.datalist;

            item.addEventListener('mousedown', function (e) {
                e.preventDefault();
            });

            item.addEventListener('click', function () {
                if (datalist.multi) {
                    datalist.select(datalist.selected.concat(data), true);
                } else {
                    datalist.select(data, true);
                }

                datalist.stopLoading();
                autocomplete.hide();
            });

            item.addEventListener('mouseenter', function () {
                if (autocomplete.activeItem) {
                    autocomplete.activeItem.classList.remove('active');
                }

                this.classList.add('active');
                autocomplete.activeItem = this;
            });
        }
    };

    return MvcDatalistAutocomplete;
}());
var MvcDatalist = (function () {
    function MvcDatalist(element, options) {
        var datalist = this;
        var group = datalist.findDatalist(element);
        if (group.dataset.id) {
            return datalist.instances[parseInt(group.dataset.id)].set(options || {});
        }

        datalist.items = [];
        datalist.group = group;
        datalist.selected = [];
        datalist.for = group.dataset.for;
        datalist.url = group.dataset.url;
        datalist.multi = group.dataset.multi == 'true';
        datalist.readonly = group.dataset.readonly == 'true';
        datalist.group.dataset.id = datalist.instances.length;
        datalist.options = { searchDelay: 300, loadingDelay: 300 };

        datalist.search = group.querySelector('.datalist-input');
        datalist.browser = group.querySelector('.datalist-browser');
        datalist.control = group.querySelector('.datalist-control');
        datalist.error = group.querySelector('.datalist-control-error');
        datalist.valueContainer = group.querySelector('.datalist-values');
        datalist.values = datalist.valueContainer.querySelectorAll('.datalist-value');

        datalist.instances.push(datalist);
        datalist.filter = new MvcDatalistFilter(datalist);
        datalist.dialog = new MvcDatalistDialog(datalist);
        datalist.autocomplete = new MvcDatalistAutocomplete(datalist);

        datalist.set(options || {});
        datalist.reload(false);
        datalist.cleanUp();
        datalist.bind();
    }

    MvcDatalist.prototype = {
        instances: [],
        lang: {
            more: 'More...',
            search: 'Search...',
            select: 'Select ({0})',
            noData: 'No data found',
            error: 'Error while retrieving records'
        },

        findDatalist: function (element) {
            var datalist = element;

            if (!datalist) {
                throw new Error('Datalist element was not specified.');
            }

            while (datalist && !datalist.classList.contains('datalist')) {
                datalist = datalist.parentElement;
            }

            if (!datalist) {
                throw new Error('Datalist can only be created from within datalist structure.');
            }

            return datalist;
        },

        extend: function () {
            var options = {};

            for (var i = 0; i < arguments.length; i++) {
                for (var key in arguments[i]) {
                    if (Object.prototype.toString.call(options[key]) == '[object Object]') {
                        options[key] = this.extend(options[key], arguments[i][key]);
                    } else {
                        options[key] = arguments[i][key];
                    }
                }
            }

            return options;
        },
        set: function (options) {
            var datalist = this;

            datalist.options.loadingDelay = options.loadingDelay == null ? datalist.options.loadingDelay : options.loadingDelay;
            datalist.options.searchDelay = options.searchDelay == null ? datalist.options.searchDelay : options.searchDelay;
            datalist.autocomplete.options = datalist.extend(datalist.autocomplete.options, options.autocomplete);
            datalist.setReadonly(options.readonly == null ? datalist.readonly : options.readonly);
            datalist.dialog.options = datalist.extend(datalist.dialog.options, options.dialog);

            return datalist;
        },
        setReadonly: function (readonly) {
            var datalist = this;
            datalist.readonly = readonly;

            if (readonly) {
                datalist.search.tabIndex = -1;
                datalist.search.readOnly = true;
                datalist.group.classList.add('datalist-readonly');

                if (datalist.browser) {
                    datalist.browser.tabIndex = -1;
                }
            } else {
                datalist.search.removeAttribute('readonly');
                datalist.search.removeAttribute('tabindex');
                datalist.group.classList.remove('datalist-readonly');

                if (datalist.browser) {
                    datalist.browser.removeAttribute('tabindex');
                }
            }

            datalist.resize();
        },

        browse: function () {
            var datalist = this;

            if (!datalist.readonly) {
                if (datalist.browser) {
                    datalist.browser.blur();
                }

                datalist.dialog.open();
            }
        },
        reload: function (triggerChanges) {
            var datalist = this;
            var originalValue = datalist.search.value;
            var ids = [].filter.call(datalist.values, function (element) {
                return element.value;
            });

            if (ids.length) {
                datalist.startLoading({ ids: ids, offset: 0, rows: ids.length }, function (data) {
                    datalist.select(data.Rows, triggerChanges);
                });
            } else {
                datalist.stopLoading();
                datalist.select([], triggerChanges);

                if (!datalist.multi && datalist.search.name) {
                    datalist.search.value = originalValue;
                }
            }
        },
        select: function (data, triggerChanges) {
            var datalist = this;
            triggerChanges = triggerChanges == null || triggerChanges;

            if (!datalist.dispatchEvent(datalist.group, 'datalistselect', { datalist: datalist, data: data, triggerChanges: triggerChanges })) {
                return;
            }

            if (triggerChanges && data.length == datalist.selected.length) {
                triggerChanges = false;
                for (var i = 0; i < data.length && !triggerChanges; i++) {
                    triggerChanges = data[i].Id != datalist.selected[i].Id;
                }
            }

            datalist.selected = data;

            if (datalist.multi) {
                datalist.search.value = '';
                datalist.valueContainer.innerHTML = '';
                datalist.items.forEach(function (item) {
                    item.parentElement.removeChild(item);
                });

                datalist.items = datalist.createSelectedItems(data);
                datalist.items.forEach(function (item) {
                    datalist.control.insertBefore(item, datalist.search);
                });

                datalist.values = datalist.createValues(data);
                datalist.values.forEach(function (value) {
                    datalist.valueContainer.appendChild(value);
                });

                datalist.resize();
            } else if (data.length) {
                datalist.values[0].value = data[0].Id;
                datalist.search.value = data[0].Label;
            } else {
                datalist.values[0].value = '';
                datalist.search.value = '';
            }

            if (triggerChanges) {
                var change = null;

                if (typeof Event === 'function') {
                    change = new Event('change');
                } else {
                    change = document.createEvent('Event');
                    change.initEvent('change', true, false);
                }

                datalist.search.dispatchEvent(change);
                [].forEach.call(datalist.values, function (value) {
                    value.dispatchEvent(change);
                });
            }
        },
        selectFirst: function (triggerChanges) {
            var datalist = this;

            datalist.startLoading({ search: '', offset: 0, rows: 1 }, function (data) {
                datalist.select(data.Rows, triggerChanges);
            });
        },
        selectSingle: function (triggerChanges) {
            var datalist = this;

            datalist.startLoading({ search: '', offset: 0, rows: 2 }, function (data) {
                if (data.Rows.length == 1) {
                    datalist.select(data.Rows, triggerChanges);
                } else {
                    datalist.select([], triggerChanges);
                }
            });
        },

        createSelectedItems: function (data) {
            var items = [];

            for (var i = 0; i < data.length; i++) {
                var button = document.createElement('button');
                button.className = 'datalist-deselect';
                button.innerText = '×';
                button.type = 'button';

                var item = document.createElement('div');
                item.innerText = data[i].Label || '';
                item.className = 'datalist-item';
                item.appendChild(button);
                items.push(item);

                this.bindDeselect(button, data[i].Id);
            }

            return items;
        },
        createValues: function (data) {
            var inputs = [];

            for (var i = 0; i < data.length; i++) {
                var input = document.createElement('input');
                input.className = 'datalist-value';
                input.value = data[i].Id;
                input.type = 'hidden';
                input.name = this.for;

                inputs.push(input);
            }

            return inputs;
        },

        startLoading: function (search, success, error) {
            var datalist = this;

            datalist.stopLoading();
            datalist.loading = setTimeout(function () {
                datalist.autocomplete.hide();
                datalist.group.classList.add('datalist-loading');
            }, datalist.options.loadingDelay);
            datalist.group.classList.remove('datalist-error');

            datalist.request = new XMLHttpRequest();
            datalist.request.open('GET', datalist.filter.formUrl(search), true);
            datalist.request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

            datalist.request.onload = function () {
                if (200 <= datalist.request.status && datalist.request.status < 400) {
                    datalist.stopLoading();

                    success(JSON.parse(datalist.request.responseText));
                } else {
                    datalist.request.onerror();
                }
            };

            datalist.request.onerror = function () {
                datalist.group.classList.add('datalist-error');
                datalist.error.title = datalist.lang.error;
                datalist.autocomplete.hide();
                datalist.stopLoading();

                if (error) {
                    error();
                }
            };

            datalist.request.send();
        },
        stopLoading: function () {
            var datalist = this;

            if (datalist.request && datalist.request.readyState != 4) {
                datalist.request.abort();
            }

            clearTimeout(datalist.loading);
            datalist.group.classList.remove('datalist-loading');
        },

        dispatchEvent: function (element, type, detail) {
            var event;

            if (typeof Event === 'function') {
                event = new CustomEvent(type, {
                    cancelable: true,
                    detail: detail,
                    bubbles: true
                });
            } else {
                event = document.createEvent('Event');
                event.initEvent(type, true, true);
                event.detail = detail;
            }

            return element.dispatchEvent(event);
        },
        bindDeselect: function (close, id) {
            var datalist = this;

            close.addEventListener('click', function () {
                datalist.select(datalist.selected.filter(function (value) {
                    return value.Id != id;
                }), true);

                datalist.search.focus();
            });
        },
        indexOf: function (selection, id) {
            for (var i = 0; i < selection.length; i++) {
                if (selection[i].Id == id) {
                    return i;
                }
            }

            return -1;
        },
        cleanUp: function () {
            var data = this.group.dataset;

            delete data.readonly;
            delete data.filters;
            delete data.dialog;
            delete data.search;
            delete data.multi;
            delete data.order;
            delete data.title;
            delete data.rows;
            delete data.sort;
            delete data.url;
        },
        resize: function () {
            var datalist = this;

            if (datalist.items.length) {
                var style = getComputedStyle(datalist.control);
                var contentWidth = datalist.control.clientWidth;
                var lastItem = datalist.items[datalist.items.length - 1];
                contentWidth -= parseFloat(style.paddingLeft) + parseFloat(style.paddingRight);
                var widthLeft = Math.floor(contentWidth - lastItem.offsetLeft - lastItem.offsetWidth);

                if (widthLeft > contentWidth / 3) {
                    style = getComputedStyle(datalist.search);
                    widthLeft -= parseFloat(style.marginLeft) + parseFloat(style.marginRight) + 4;
                    datalist.search.style.width = widthLeft + 'px';
                } else {
                    datalist.search.style.width = '';
                }
            } else {
                datalist.search.style.width = '';
            }
        },
        bind: function () {
            var datalist = this;

            window.addEventListener('resize', function () {
                datalist.resize();
            });

            datalist.search.addEventListener('focus', function () {
                datalist.group.classList.add('datalist-focus');
            });

            datalist.search.addEventListener('blur', function () {
                datalist.stopLoading();
                datalist.autocomplete.hide();
                datalist.group.classList.remove('datalist-focus');

                var originalValue = this.value;
                if (!datalist.multi && datalist.selected.length) {
                    if (datalist.selected[0].Label != this.value) {
                        datalist.select([], true);
                    }
                } else {
                    this.value = '';
                }

                if (!datalist.multi && datalist.search.name) {
                    this.value = originalValue;
                }
            });

            datalist.search.addEventListener('keydown', function (e) {
                switch (e.which) {
                    case 8:
                        if (!this.value.length && datalist.selected.length) {
                            datalist.select(datalist.selected.slice(0, -1), true);
                        }

                        break;
                    case 9:
                        if (datalist.autocomplete.activeItem) {
                            if (datalist.browser) {
                                datalist.browser.tabIndex = -1;

                                setTimeout(function () {
                                    datalist.browser.removeAttribute('tabindex');
                                }, 100);
                            }

                            datalist.autocomplete.activeItem.click();
                        }

                        break;
                    case 13:
                        if (datalist.autocomplete.activeItem) {
                            e.preventDefault();

                            datalist.autocomplete.activeItem.click();
                        }

                        break;
                    case 38:
                        e.preventDefault();

                        datalist.autocomplete.previous();

                        break;
                    case 40:
                        e.preventDefault();

                        datalist.autocomplete.next();

                        break;
                }
            });
            datalist.search.addEventListener('input', function () {
                if (!this.value.length && !datalist.multi && datalist.selected.length) {
                    datalist.autocomplete.hide();
                    datalist.select([], true);
                }

                datalist.autocomplete.search(this.value);
            });

            if (datalist.browser) {
                datalist.browser.addEventListener('click', function () {
                    datalist.browse();
                });
            }

            for (var i = 0; i < datalist.filter.additional.length; i++) {
                var inputs = document.querySelectorAll('[name="' + datalist.filter.additional[i] + '"]');

                for (var j = 0; j < inputs.length; j++) {
                    inputs[j].addEventListener('change', function () {
                        if (!datalist.dispatchEvent(this, 'filterchange', { datalist: datalist })) {
                            return;
                        }

                        datalist.stopLoading();
                        datalist.filter.offset = 0;

                        var ids = [].filter.call(datalist.values, function (element) {
                            return element.value;
                        });

                        if (ids.length || datalist.selected.length) {
                            datalist.startLoading({ checkIds: ids, offset: 0, rows: ids.length }, function (data) {
                                datalist.select(data.Rows, true);
                            }, function () {
                                datalist.select([], true);
                            });
                        }
                    });
                }
            }
        }
    };

    return MvcDatalist;
}());
