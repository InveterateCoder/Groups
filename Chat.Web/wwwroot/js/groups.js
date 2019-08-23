﻿var app;
document.addEventListener('DOMContentLoaded', () => {
    app = new app_class();
    window.addEventListener('resize', () => {
        app.on_resize();
    });
});
class api_class {
    constructor() { }
    async register(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('/api/account/reg', chatterer);
            switch (resp) {
                case "pending_" + chatterer.email:
                    ret = true;
                    break;
                case "signed_out":
                    app.alert("You've been signed out, try again");
                    break;
                case "email_is_taken":
                    app.alert("This email address has been already registered");
                    break;
                case "name_is_taken":
                    app.alert("This nickname has been already taken");
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async validate(number) {
        try {
            let ret = false;
            let resp = await this.post('api/account/val', number);
            if (resp.startsWith('added_'))
                ret = true;
            else if (resp == 'signed_out')
                app.alert("You've been signed out, try again");
            else if (resp == 'invalid_confirmation_id')
                app.alert('Wrong number');
            else if (resp == 'reg_request_required')
                app.alert('Registration is required prior to email verification');
            else if (resp == 'email_is_taken')
                app.alert("This email address has been already registered");
            else if (resp == 'name_is_taken')
                app.alert("This nickname has been already taken");
            else app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async signin(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('api/account/sign', chatterer);
            if (resp == "OK")
                ret = await this.usr_info();
            else if (resp == 'signed_out')
                app.alert("You've been signed out, try again");
            else if (resp == 'user_not_found')
                app.alert("The email address is not registered");
            else if (resp == 'password_incorrect')
                app.alert('Wrong password');
            else if (resp == 'multiple_signins_forbidden')
                app.alert('Access denied, already signed in');
            else
                app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async signout() {
        try {
            let ret = false;
            let resp = await this.post('api/account/user/signout');
            if (resp == 'OK' || resp == "not_authorized" || resp == "single_connection_only")
                ret = true;
            else
                app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async acc_change(request) {
        try {
            let ret = {
                name: false,
                pass: false
            };
            let resp = await this.post('api/account/user/change', request);
            switch (resp) {
                case "name_changed":
                    ret.name = true;
                    app.name = request.NewName;
                    break;
                case "pass_changed":
                    ret.pass = true;
                    break;
                case "name&pass_changed":
                    ret.name = true;
                    ret.pass = true;
                    app.name = request.NewName;
                    break;
                case "wrong_password":
                    app.alert("Wrong password")
                    break;
                case "name_exists":
                    app.alert("The name is already taken, choose another one");
                case "no_change_requested":
                    app.alert("Empty request");
                    break;
                case "same_credentials":
                    app.alert("No changes have been made, same credentials");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    ret.pass = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async acc_delete(signin) {
        try {
            let ret = false;
            let resp = await this.post('api/account/user/del', signin);
            switch (resp) {
                case "deleted":
                    ret = true;
                    break;
                case "wrong_email":
                    app.alert("Wrong email address");
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_reg(info) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/reg", info);
            switch (resp) {
                case "OK":
                    app.group = info.GroupName;
                    ret = true;
                    break;
                case "has_group":
                    app.alert("You have already registered a group");
                    break;
                case "name_taken":
                    app.alert("A group with such name already exists");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    ret = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_change(info) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/change", info);
            switch (resp) {
                case "name_changed":
                case "name&pass_changed":
                    app.group = info.NewGroupName;
                case "pass_changed":
                case "not_authorized":
                case "single_connection_only":
                    ret = true;
                    break;
                case "has_no_group":
                    app.group = null;
                    ret = true;
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "group_name_exists":
                    app.alert("The group name is already taken, choose another one");
                    break;
                case "not_changed":
                    app.alert("No changes were made");
                    break;
                case "no_change_requested":
                    app.alert("No change was requested");
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async usr_info() {
        try {
            let ret = false;
            let fetresp = await fetch('api/account/user/info', {
                method: 'get',
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (fetresp.status != 200) {
                app.name = null;
                app.group = null;
                app.goto('reg');
            }
            else {
                let obj = await fetresp.json();
                if (obj.name != null) {
                    app.name = obj.name;
                    ret = true;
                }
                else
                    app.name = null;
                if (obj.group != null)
                    app.group = obj.group;
                else
                    app.group = null;
                return ret;
            }
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_del(pass) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/del", pass);
            switch (resp) {
                case "deleted":
                case "has_no_group":
                    app.group = null;
                    ret = true;
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async list_groups(start, quantity, query) {
        let ret = null;
        if (start != 0 && !start || isNaN(start) || start < 0)
            return null;
        if (!quantity && quantity != 0)
            ret = await this.get(`api/groups/list/${start}`);
        else {
            if (isNaN(quantity) || quantity < 1 || quantity > 100)
                return null;
            if (!query && query != 0)
                ret = await this.get(`api/groups/list/${start}/${quantity}`);
            else {
                if (query.length > 64)
                    return null;
                else
                    ret = await this.get(`api/groups/list/${start}/${quantity}/${query}`);
            }
        }
        return ret;
    }
    async post(addr, obj) {
        app.wait();
        let ret;
        try {
            let resp = await fetch(addr, {
                method: 'post',
                headers: {
                    'Accept': 'plain/text', 'Content-Type': 'application/json'
                },
                body: JSON.stringify(obj || '')
            });
            if (resp.status != 200) {
                app.name = null;
                app.group = null;
                app.goto('reg');
            }
            ret = await resp.text();
        }
        catch (err) {
            ret = err.message;
        }
        finally {
            app.resume();
            return ret;
        }
    }
    async get(addr) {
        let ret;
        try {
            let resp = await fetch(addr, {
                method: 'get',
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (resp.status != 200) {
                app.name = null;
                app.group = null;
                app.goto('reg');
                ret = null;
            }
            else
                ret = await resp.json();
        }
        catch (err) {
            ret.response = err.message;
        }
        finally {
            return ret;
        }
    }
}
class reg_panel_class {
    constructor() {
        this.panel = document.getElementById('reg_panel');
        this.place = 'home';;
    }
    num(e) {
        if (isNaN(e.key))
            e.preventDefault();
    }
    pasteFilter(e) {
        if (isNaN(e.target.value))
            e.target.value = null;
    }
    clear() {
        let arr;
        switch (this.place) {
            case 'reg':
                arr = this.panel.children[2].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'sign':
                arr = this.panel.children[3].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'numb':
                this.panel.children[4].getElementsByTagName('input')[0].value = null;
                break;
        }
    }
    btn_hover(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    btn_out(el) {
        el.firstElementChild.style.transform = "scale(1,1)";
    }
    btn_down(el) {
        el.firstElementChild.style.transform = "scale(0.9,0.9)";
    }
    btn_up(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    goto(place) {
        switch (place) {
            case 'home':
                this.hide();
                this.panel.children[1].style.display = 'flex';
                this.place = place;
                break;
            case 'reg':
                this.hide();
                this.panel.children[2].style.display = 'flex';
                this.place = place;
                break;
            case 'sign':
                this.hide();
                this.panel.children[3].style.display = 'flex';
                this.place = place;
                break;
            case 'numb':
                this.hide();
                this.panel.children[4].style.display = 'flex';
                this.place = place;
                break;
        }
    }
    hide() {
        switch (this.place) {
            case 'home':
                this.panel.children[1].style.display = 'none';
                break;
            case 'reg':
                this.clear();
                this.panel.children[2].style.display = 'none';
                break;
            case 'sign':
                this.clear();
                this.panel.children[3].style.display = 'none';
                break;
            case 'numb':
                this.clear();
                this.panel.children[4].style.display = 'none';
                break;
        }
    }
    register() {
        let form = this.panel.children[2].getElementsByTagName('input');
        let chatterer = {
            name: form[0].value,
            email: form[1].value,
            password: form[2].value
        };
        if (!app.validateName(chatterer.name))
            app.alert(app.message.name("Name"));
        else if (!chatterer.email || !form[1].checkValidity())
            app.alert('Incorrect email address');
        else if (!app.validatePassword(chatterer.password))
            app.alert(app.message.password("Password"));
        else {
            app.api.register(chatterer).then(ok => {
                if (ok)
                    this.goto("numb");
            });
        }
    }
    validate() {
        let num = this.panel.children[4].getElementsByTagName('input')[0].value;
        if (num.length != 4)
            app.alert("Code must consist of 4 digits");
        else {
            app.api.validate(Number(num)).then(ok => {
                if (ok) {
                    this.goto("sign");
                }
            });
        }
    }
    signin() {
        let form = this.panel.children[3].getElementsByTagName('input');
        let chatterer = {
            email: form[0].value,
            password: form[1].value
        };
        if (!chatterer.email || !form[0].checkValidity())
            app.alert("Incorrect email address");
        else if (!app.validatePassword(chatterer.password))
            app.alert(app.message.password("Password"));
        else app.api.signin(chatterer).then(ok => {
            if (ok)
                app.goto('groups');
        });
    }
}

class groups_class {
    constructor() {
        this.groups_window = document.getElementById('groups');
        this.hmbrgr_btn = document.getElementById('hmbrgr');
        this.m_acc = document.getElementById('m_account');
        this.m_group = document.getElementById('m_group');
        this.m_all = document.getElementById('m_all');
        this.account_btn = this.groups_window.children[2].children[2].children[0].children[0];
        this.group_btn = this.groups_window.children[2].children[2].children[0].children[1];
        this.acc_open = false;
        this.group_open = false;
        this.groups_forms = document.getElementById('groups_forms');
        this.start = 0;
        this.quantity = 100;
        this.query = '';
    }
    hmbrg_click() {
        this.hmbrgr_btn.classList.toggle('clicked');
        if (this.hmbrgr_btn.classList.contains('clicked')) {
            this.m_all.style.opacity = '1';
            this.m_all.style.transform = 'translate(0,0)';
        }
        else {
            this.m_all.style.opacity = '0';
            this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
        }
    }
    hmbrg_over() {
        for (let i = 0; i < this.hmbrgr_btn.children.length; i++) {
            this.hmbrgr_btn.children[i].style.backgroundColor = 'white';
            this.hmbrgr_btn.children[i].style.boxShadow = '0 0 4px white';
        }
    }
    hmbrg_out() {
        for (let i = 0; i < this.hmbrgr_btn.children.length; i++) {
            this.hmbrgr_btn.children[i].style.backgroundColor = 'silver';
            this.hmbrgr_btn.children[i].style.boxShadow = 'none';
        }
    }
    initialize(group = true) {
        let height = this.groups_window.children[2].offsetHeight;
        this.groups_window.children[0].style.top = height + 'px';
        this.m_acc.style.right = '0px';
        this.m_acc.style.top = height + 'px';
        this.m_acc.style.width = this.account_btn.offsetWidth + 28 + 'px';
        this.m_group.style.right = this.m_acc.offsetWidth + 'px';
        this.m_group.style.top = height + 'px';
        this.m_group.style.width = this.group_btn.offsetWidth + 28 + 'px';
        if (group)
            this.group_conf();
        this.m_all.style.top = this.groups_window.children[2].offsetHeight + 'px';
        this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
    }
    group_conf() {
        if (app.group) {
            this.m_group.children[0].style.display = 'none';
            this.m_group.children[1].textContent = app.group;
            this.m_group.children[1].title = app.group;
            this.m_group.children[1].style.display = 'list-item';
            this.m_group.children[2].style.display = 'list-item';
            this.m_group.children[3].style.display = 'list-item';
            this.m_all.children[5].style.display = 'none';
            this.m_all.children[6].textContent = app.group;
            this.m_all.children[6].title = app.group;
            this.m_all.children[6].style.display = 'block';
            this.m_all.children[7].style.display = 'block';
            this.m_all.children[8].style.display = 'block';
        }
        else {
            this.m_group.children[0].style.display = 'list-item';
            this.m_group.children[1].style.display = 'none';
            this.m_group.children[2].style.display = 'none';
            this.m_group.children[3].style.display = 'none';
            this.m_all.children[5].style.display = 'block';
            this.m_all.children[6].style.display = 'none';
            this.m_all.children[7].style.display = 'none';
            this.m_all.children[8].style.display = 'none';
        }
    }
    m_accf() {
        this.m_proc(this.m_acc, this.account_btn, this.acc_open, true);
    }
    m_groupf() {
        this.m_proc(this.m_group, this.group_btn, this.group_open);
    }
    m_proc(el, btn, open, isacc = false) {
        if (open) {
            btn.classList.remove('open');
            if (isacc)
                this.acc_open = false;
            else
                this.group_open = false;
            el.style.opacity = '0';
            el.style.visibility = 'hidden';
        }
        else {
            btn.classList.add('open');
            el.style.visibility = 'visible';
            el.style.opacity = '1';
            el.focus();
            if (isacc)
                this.acc_open = true;
            else
                this.group_open = true;
        }
    }
    m_onblur(el) {
        el.style.opacity = '0';
        if (el.id == 'm_account')
            this.account_btn.classList.remove('open');
        else
            this.group_btn.classList.remove('open');
        setTimeout(() => {
            if (el.id == 'm_account') {
                this.acc_open = false;
                this.m_acc.style.visibility = 'hidden';
            }
            else {
                this.group_open = false;
                this.m_group.style.visibility = 'hidden';
            }
        }, 170);
    }
    close_all_menus() {
        if (this.acc_open) {
            this.account_btn.classList.remove('open');
            this.acc_open = false;
            this.m_acc.style.opacity = '0';
            this.m_acc.style.visibility = 'hidden';
        }
        else if (this.group_open) {
            this.group_btn.classList.remove('open');
            this.group_open = false;
            this.m_group.style.opacity = '0';
            this.m_group.style.visibility = 'hidden';
        }
        if (this.hmbrgr_btn.classList.contains('clicked')) {
            this.hmbrgr_btn.classList.remove('clicked');
            this.m_all.style.opacity = '0';
            this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
        }
    }
    groups_resize() {
        this.close_all_menus();
        this.initialize(false);
    }
    signout() {
        this.close_all_menus();
        app.api.signout().then(ok => {
            if (ok) {
                app.goto('reg');
                app.name = null;
                app.group = null;
            }
        });
    }
    form_open(n) {
        this.groups_forms.children[n].style.display = 'flex';
        this.groups_forms.style.display = 'block';
        this.close_all_menus();
    }
    form_close(n) {
        this.groups_forms.style.display = 'none';
        this.groups_forms.children[n].style.display = 'none';
        let form = this.groups_forms.children[n].getElementsByTagName('input');
        if (n == 4) {
            this.groups_forms.children[4].children[4].style.visibility = 'visible';
            form[2].checked = false;
            for (let i = 0; i < form.length; i++) {
                if (i == 2) continue;
                form[i].value = null;
            };
        }
        else
            for (let i = 0; i < form.length; i++) {
                form[i].value = null;
            };
    }
    acc_change() {
        let form = this.groups_forms.children[1].getElementsByTagName('input');
        let request = {
            Password: form[0].value,
            NewName: form[1].value,
            NewPassword: form[2].value
        };
        if (!app.validatePassword(request.Password))
            app.alert(app.message.password("Password"));
        else if (request.NewName.length > 0 && !app.validateName(request.NewName))
            app.alert(app.message.name("New name"));
        else if (request.NewPassword.length > 0 && !app.validatePassword(request.NewPassword))
            app.alert(app.message.password("New password"));
        else if (request.Password == request.NewPassword || request.NewName == app.name)
            app.alert("Leave blank if the credentials are the same");
        else if (!request.NewName && !request.NewPassword)
            app.alert("No change requested");
        else {
            if (request.NewName.length == 0)
                request.NewName = null;
            if (request.NewPassword.length == 0)
                request.NewPassword = null;
            app.api.acc_change(request).then(ret => {
                if (ret.name || ret.pass) {
                    if (ret.name)
                        this.groups_window.getElementsByTagName('code')[0].textContent = app.name;
                    this.form_close(1);
                }
            });
        }
    }
    acc_delete() {
        let form = this.groups_forms.children[2].getElementsByTagName('input');
        let signin = {
            email: form[0].value,
            password: form[1].value
        };
        if (!signin.email || !form[0].checkValidity())
            app.alert("Incorrect email address");
        else if (!app.validatePassword(signin.password))
            app.alert(app.message.password("Password"));
        else {
            app.api.acc_delete(signin).then(ret => {
                if (ret) {
                    app.goto('reg');
                    app.name = null;
                    app.group = null;
                    this.form_close(2);
                }
            });
        }
    }
    group_create() {
        let form = this.groups_forms.children[3].getElementsByTagName('input');
        let info = {
            Password: form[0].value,
            GroupName: form[1].value,
            GroupPassword: form[2].value
        };
        if (!app.validatePassword(info.Password))
            app.alert(app.message.password("Password"));
        else if (!app.validateName(info.GroupName))
            app.alert(app.message.name("Group name"));
        else if (info.GroupPassword.length > 0 && !app.validatePassword(info.GroupPassword))
            app.alert(app.message.password("Group password"));
        else {
            if (info.GroupPassword.length == 0)
                info.GroupPassword = null;
            app.api.grp_reg(info).then(ret => {
                if (ret) {
                    this.group_conf();
                    this.form_close(3);
                }
            });
        }
    }
    group_change() {
        let form = this.groups_forms.children[4].getElementsByTagName('input');
        let info = {
            Password: form[0].value,
            NewGroupName: form[1].value,
            NewGroupPassword: form[3].value
        };
        if (!app.validatePassword(info.Password))
            app.alert(app.message.password("Password"));
        else if (info.NewGroupName.length > 0 && !app.validateName(info.NewGroupName))
            app.alert(app.message.name("New name"));
        else if (!form[2].checked && info.NewGroupPassword.length > 0 && !app.validatePassword(info.NewGroupPassword))
            app.alert(app.message.password("New password"));
        else {
            if (form[2].checked) info.NewGroupPassword = null;
            else if (info.NewGroupPassword.length == 0) info.NewGroupPassword = "000";
            if (info.NewGroupName.length == 0)
                info.NewGroupName = null; 
            if (!info.NewGroupName && info.NewGroupPassword == "000")
                app.alert("No change requested");
            else {
                app.api.grp_change(info).then(ret => {
                    if (ret) {
                        this.group_conf();
                        this.form_close(4);
                    }
                });
            }
        }

    }
    group_del() {
        let pass = this.groups_forms.children[5].getElementsByTagName('input')[0].value;
        if (!app.validatePassword(pass))
            app.alert(app.message.password("Password"));
        else app.api.grp_del(pass).then(ret => {
            if (ret) {
                this.group_conf();
                this.form_close(5);
            }
        });
    }
    on_pass_del_change(el) {
        if (el.checked)
            this.groups_forms.children[4].children[4].style.visibility = 'hidden';
        else
            this.groups_forms.children[4].children[4].style.visibility = 'visible';
    }
    list_clear() {
        let div = this.groups_window.firstElementChild;
        if (div.childElementCount > 0)
            while (div.firstElementChild)
                div.firstElementChild.remove();
        this.start = 0;
    }
    list_add(list) {
        if (!list || !Array.isArray(list) || list.length == 0)
            return false;
        let div = this.groups_window.firstElementChild;
        list.forEach((str) => {
            if (typeof str == "string") {
                let node = document.createElement("button");
                var textnode = document.createTextNode(str);
                node.appendChild(textnode);
                node.onclick = () => app.groups.on_group_clicked(str);
                node.tabIndex = -1;
                div.appendChild(node);
            }
        });
    }
    list_load() { //todo loading ring
        if (this.quantity > 0) {
            app.api.list_groups(this.start, this.quantity, this.query).then(ret => {
                if (ret == 0)
                    this.quantity = 0;
                else {
                    if (typeof ret == "string")
                        app.alert(ret);
                    else if (Array.isArray(ret)) {
                        this.start += ret.length;
                        if (ret.length < this.quantity)
                            this.quantity = 0;
                        this.list_add(ret);
                    }
                }
            });
        }
    }
    on_scroll(el) {
        if (el.scrollTop + el.clientHeight >= el.scrollHeight) { //todo create loading buffer
            app.alert("here");
        }
    }
    on_group_clicked(group) {
        app.alert(group);
    }
}

class app_class {
    constructor() {
        this.page = null;
        this.name = null;
        this.group = null;
        this.api = new api_class();
        this.reg_panel = new reg_panel_class();
        this.groups = new groups_class();
        setTimeout(() => {
            this.api.usr_info().then(ret => {
                document.body.style.backgroundPosition = 'unset';
                document.body.style.backgroundRepeat = 'unset';
                document.body.style.backgroundAttachment = 'unset';
                if (ret)
                    this.goto('groups');
            });
        }, 1300);
        this.message = {
            password: text => text + " must be at least 8 characters long and maximum 32",
            name: text => text + " must be at least 5 characters long and maximum 64"
        };
    }
    on_resize() {
        this.groups.groups_resize();
    }
    wait() {
        document.getElementById('wait').style.visibility = 'visible';
    }
    resume() {
        document.getElementById('wait').style.visibility = 'hidden';
    }
    alert(message) {
        let el = document.getElementById('message');
        el.children[0].children[0].textContent = message;
        el.style.display = 'block';
    }
    goto(place) {
        if (this.page == place)
            return;
        switch (place) {
            case 'reg':
                document.body.style.backgroundColor = 'rgb(0, 3, 56)';
                document.body.style.backgroundImage = 'url("/images/concrete-wall-2.png")';
                document.body.children[0].style.display = 'block';
                this.hide('reg');
                break;
            case 'groups':
                this.groups.list_clear();
                this.groups.list_load();
                document.body.style.backgroundColor = '#efefef';
                document.body.style.backgroundImage = 'url("/images/low-contrast-linen.png")';
                this.groups.groups_window.getElementsByTagName('code')[0].textContent = this.name;
                this.groups.groups_window.getElementsByTagName('code')[0].title = this.name;
                document.body.children[1].style.display = 'block';
                this.groups.initialize();
                this.hide('groups');
                break;
            case 'ingroup':
                document.body.children[2].style.display = 'block';
                this.hide('ingroup');
                break;
        }
    }
    hide(place) {
        switch (this.page) {
            case 'reg':
                document.body.children[0].style.display = 'none';
                try {
                    app.reg_panel.goto('home');
                }
                catch (err) { }
                break;
            case 'groups':
                document.body.children[1].style.display = 'none';
                break;
            case 'ingroup':
                document.body.children[2].style.display = 'none';
                break;
        }
        this.page = place;
    }
    validatePassword(password) {
        if (!password || password.length < 8 || password.length > 32)
            return false;
        else
            return true;
    }
    validateName(name) {
        if (!name || name.length < 5 || name.length > 64)
            return false;
        else
            return true;
    }
}